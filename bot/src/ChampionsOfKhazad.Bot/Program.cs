using ChampionsOfKhazad.Bot;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

var host = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(host.Environment.IsProduction() ? LogEventLevel.Information : LogEventLevel.Debug)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

host.Logging.ClearProviders().AddSerilog();

host.Services
    .AddOptions<BotOptions>()
    .Bind(host.Configuration.GetSection(BotOptions.Key))
    .ValidateDataAnnotations()
    .ValidateOnStart();

host.Services.AddSingleton<DiscordSocketClient>(
    services =>
        ActivatorUtilities.CreateInstance<LoggingDiscordSocketClient>(
            services,
            new DiscordSocketConfig
            {
                MessageCacheSize = 100,
                GatewayIntents =
                    GatewayIntents.Guilds
                    | GatewayIntents.GuildMessages
                    | GatewayIntents.DirectMessages
                    | GatewayIntents.MessageContent,
                LogLevel = LogSeverity.Debug
            }
        )
);

var messageHandlerOptions = host.Configuration
    .GetSection(MessageHandlerOptions.Key)
    .Get<MessageHandlerOptions>();

if (messageHandlerOptions is not null)
{
    foreach (var emoteStreakOptions in messageHandlerOptions.EmoteStreak)
    {
        if (emoteStreakOptions.EmoteName == default)
            throw new ArgumentException(
                $"{MessageHandlerOptions.Key}:{nameof(MessageHandlerOptions.EmoteStreak)}:{nameof(EmoteStreakMessageHandlerOptions.EmoteName)} is required"
            );

        if (emoteStreakOptions.ChannelId == default)
            throw new ArgumentException(
                $"{MessageHandlerOptions.Key}:{nameof(MessageHandlerOptions.EmoteStreak)}:{nameof(EmoteStreakMessageHandlerOptions.ChannelId)} is required"
            );

        if (emoteStreakOptions.BotId == default)
            throw new ArgumentException(
                $"{MessageHandlerOptions.Key}:{nameof(MessageHandlerOptions.EmoteStreak)}:{nameof(EmoteStreakMessageHandlerOptions.BotId)} is required"
            );

        host.Services.AddSingleton<IMessageHandler>(
            new EmoteStreakMessageHandler(emoteStreakOptions)
        );
    }
}

host.Services.AddSingleton<IMessageHandler, DirectMessageHandler>();

host.Services.AddHostedService<BotService>();
host.Build().Run();
