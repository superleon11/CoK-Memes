using Discord;

namespace ChampionsOfKhazad.Bot;

public interface IMessageReceivedEventHandler : IEventHandler
{
    Task HandleMessageAsync(IUserMessage message);
}
