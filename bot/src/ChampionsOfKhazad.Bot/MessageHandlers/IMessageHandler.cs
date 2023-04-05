using Discord;

namespace ChampionsOfKhazad.Bot;

public interface IMessageHandler
{
    Task HandleMessageAsync(IUserMessage message);
}
