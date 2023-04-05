using System.ComponentModel.DataAnnotations;

namespace ChampionsOfKhazad.Bot;

public class EmoteStreakMessageHandlerOptions
{
    [Required]
    public required string EmoteName { get; set; }

    [Required]
    public required ulong ChannelId { get; set; }

    public bool AllowSingleUserStreaks { get; set; }
}
