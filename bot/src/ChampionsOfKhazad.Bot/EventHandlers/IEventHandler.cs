namespace ChampionsOfKhazad.Bot;

public interface IEventHandler
{
    Task StartAsync(BotContext context) => Task.CompletedTask;
}
