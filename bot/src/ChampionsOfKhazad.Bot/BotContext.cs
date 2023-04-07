using Discord;

namespace ChampionsOfKhazad.Bot;

public class BotContext
{
    public ulong BotId { get; }
    public IGuild Guild { get; }

    public BotContext(ulong botId, IGuild guild)
    {
        BotId = botId;
        Guild = guild;
    }
}
