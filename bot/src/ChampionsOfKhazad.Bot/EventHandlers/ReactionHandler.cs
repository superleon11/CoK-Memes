using Discord;
using Microsoft.Extensions.Options;

namespace ChampionsOfKhazad.Bot;

public class ReactionHandler : IMessageReceivedEventHandler
{
    private static readonly Emoji ReactionEmoji = new("🤡");
    private readonly ReactionHandlerOptions _options;

    public ReactionHandler(IOptions<ReactionHandlerOptions> options)
    {
        _options = options.Value;
    }

    public async Task HandleMessageAsync(IUserMessage message)
    {
        if (
            message.Channel is ITextChannel
            && message.Author.Id == _options.UserId
            && RandomUtils.RollTheDice(1)
        )
            await message.AddReactionAsync(ReactionEmoji);
    }

    public override string ToString() => $"{nameof(ReactionHandler)} - {_options.UserId}";
}
