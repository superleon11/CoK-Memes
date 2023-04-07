using Discord;

namespace ChampionsOfKhazad.Bot;

public class BotContext
{
    public IGuild Guild { get; }

    public BotContext(IGuild guild)
    {
        Guild = guild;
    }
}
