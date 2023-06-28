namespace ChampionsOfKhazad.Bot.ChatBot;

public record User
{
    public required ulong Id { get; init; }

    public required string Name { get; init; }
}
