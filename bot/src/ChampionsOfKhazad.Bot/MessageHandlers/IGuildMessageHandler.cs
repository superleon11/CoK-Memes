using Discord;

namespace ChampionsOfKhazad.Bot;

public interface IGuildMessageHandler : IMessageHandler
{
    Task StartAsync(IGuild guild);
}
