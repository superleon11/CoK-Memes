namespace ChampionsOfKhazad.Bot;

public class MessageHandlerOptions
{
    public const string Key = "MessageHandlers";

    public IReadOnlyCollection<EmoteStreakMessageHandlerOptions> EmoteStreak { get; set; } =
        Array.Empty<EmoteStreakMessageHandlerOptions>();
}
