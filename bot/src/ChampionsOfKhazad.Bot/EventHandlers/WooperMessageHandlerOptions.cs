using System.ComponentModel.DataAnnotations;

namespace ChampionsOfKhazad.Bot;

public class WooperMessageHandlerOptions
{
    public const string Key = "Wooper";

    [Required]
    public required ulong UserId { get; set; }
}
