using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChampionsOfKhazad.Bot;

public class BotService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly ILogger _logger;
    private readonly IEnumerable<IMessageHandler> _messageHandlers;
    private readonly BotOptions _options;

    private List<IMessageHandler> _startedMessageHandlers = new();

    public BotService(
        DiscordSocketClient client,
        ILogger<BotService> logger,
        IOptions<BotOptions> options,
        IEnumerable<IMessageHandler> messageHandlers
    )
    {
        _client = client;
        _logger = logger;
        _messageHandlers = messageHandlers;
        _options = options.Value;

        _client.Ready += Ready;
        _client.MessageReceived += MessageReceivedAsync;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Bot");

        await _client.LoginAsync(TokenType.Bot, _options.Token);

        cancellationToken.ThrowIfCancellationRequested();

        await _client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
    }

    private async Task Ready()
    {
        var guild = _client.GetGuild(_options.GuildId);

        _logger.LogDebug("Guilds: {Guilds}", _client.Guilds.Select(x => x.Id));
        _logger.LogDebug(
            "Guild: {Guild}, channels: {Channels}",
            guild.Name,
            guild.Channels.Select(x => x.Name)
        );

        _startedMessageHandlers = _messageHandlers
            .Where(x => x is not IGuildMessageHandler)
            .ToList();

        foreach (var messageHandler in _messageHandlers.OfType<IGuildMessageHandler>())
        {
            try
            {
                await messageHandler.StartAsync(guild);
                _startedMessageHandlers.Add(messageHandler);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    "Error starting message handler {MessageHandler}",
                    messageHandler
                );
            }
        }

        _logger.LogInformation("Bot started");
    }

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        if (message is not SocketUserMessage userMessage || message.Author.IsBot)
            return;

        foreach (var messageHandler in _startedMessageHandlers)
        {
            try
            {
                await messageHandler.HandleMessageAsync(userMessage);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error handling message with {MessageHandler}", messageHandler);
            }
        }
    }
}
