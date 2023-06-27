using Discord;
using Microsoft.Extensions.Options;

namespace ChampionsOfKhazad.Bot;

public class WooperMessageHandler : IMessageReceivedEventHandler
{
    private readonly WooperMessageHandlerOptions _options;
    private IEmote? _emote;

    public WooperMessageHandler(IOptions<WooperMessageHandlerOptions> options)
    {
        _options = options.Value;
    }

    public async Task StartAsync(BotContext context)
    {
        _emote = await context.Guild.GetEmotesAsync().SingleAsync(x => x.Name == "gigachad");
    }

    public async Task HandleMessageAsync(IUserMessage message)
    {
        if (message.Author.Id == _options.UserId)
        {
            await message.AddReactionAsync(_emote);
        }
    }

    public override string ToString() => nameof(WooperMessageHandler);
}
