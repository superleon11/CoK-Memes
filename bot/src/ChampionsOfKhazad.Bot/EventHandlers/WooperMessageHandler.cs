using Discord;
using Microsoft.Extensions.Options;

namespace ChampionsOfKhazad.Bot;

public class WooperMessageHandler : IMessageReceivedEventHandler
{
    private readonly WooperMessageHandlerOptions _options;
    private readonly Emoji _clown = new("🤡");
    private readonly Emoji _skull = new("💀");
    private readonly Emoji _nerd = new("🤓");

    public WooperMessageHandler(IOptions<WooperMessageHandlerOptions> options)
    {
        _options = options.Value;
    }

    public async Task HandleMessageAsync(IUserMessage message)
    {
        if (message.Author.Id == _options.UserId)
        {
            await message.AddReactionsAsync(new[] { _clown, _skull, _nerd });
        }
    }
}
