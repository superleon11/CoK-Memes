using ChampionsOfKhazad.Bot;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

var host = Host.CreateApplicationBuilder(args);

// csharpier-ignore
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(host.Environment.IsProduction() ? LogEventLevel.Information : LogEventLevel.Debug)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

host.Logging.ClearProviders().AddSerilog();

host.Services.AddOptionsWithEagerValidation<BotOptions>(
    host.Configuration.GetSection(BotOptions.Key)
);

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

host.Services
    .AddEventHandler<DirectMessageHandler>()
    .AddEventHandler<EmoteStreakHandler, EmoteStreakHandlerOptions>(
        host.Configuration.GetEventHandlerSection(EmoteStreakHandlerOptions.Key)
    )
    .AddEventHandler<SummonUserHandler, SummonUserHandlerOptions>(
        host.Configuration.GetEventHandlerSection(SummonUserHandlerOptions.Key)
    )
    .AddEventHandler<ReactionHandler, ReactionHandlerOptions>(
        host.Configuration.GetEventHandlerSection(ReactionHandlerOptions.Key)
    );

host.Services.AddHostedService<BotService>();
host.Build().Run();
