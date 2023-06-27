using System.ComponentModel.DataAnnotations;

namespace ChampionsOfKhazad.Bot;

public class SummonUserHandlerOptions
{
    public const string Key = "SummonUser";

    [Required]
    public ulong UserId { get; set; }

    [Required]
    public ulong LeaderId { get; set; }

    public bool AllowSingleUserStreaks { get; set; }
}
