using Discord;

namespace ChampionsOfKhazad.Bot;

public class EmoteStreakMessageHandler : IGuildMessageHandler
{
    private readonly string _emoteName;
    private readonly ulong _channelId;
    private readonly bool _allowSingleUserStreaks;
    private Emote? _emote;

    public EmoteStreakMessageHandler(EmoteStreakMessageHandlerOptions options)
    {
        _emoteName = options.EmoteName;
        _channelId = options.ChannelId;
        _allowSingleUserStreaks = options.AllowSingleUserStreaks;
    }

    public async Task StartAsync(IGuild guild)
    {
        var emotes = await guild.GetEmotesAsync();
        _emote = emotes.Single(x => x.Name == _emoteName);
    }

    public async Task HandleMessageAsync(IUserMessage message)
    {
        if (_emote is null)
            throw new InvalidOperationException(
                $"{nameof(EmoteStreakMessageHandler)} has not been started"
            );

        if (
            message.Channel is not ITextChannel textChannel
            || (textChannel.CategoryId != _channelId && textChannel.Id != _channelId)
            || message.Content == _emote.ToString()
        )
            return;

        var streak = 0;
        var previousAuthorId = message.Author.Id;

        await foreach (var previousMessage in message.GetPreviousMessagesAsync())
        {
            // Ignore messages that aren't from users
            if (
                previousMessage is not IUserMessage previousUserMessage
                || previousMessage.Author.IsBot
            )
                continue;

            // Streak is broken if the message isn't the emote - stop counting
            if (previousUserMessage.Content != _emote.ToString())
                break;

            // Ignore repeat messages from the same user
            if (!_allowSingleUserStreaks && previousUserMessage.Author.Id == previousAuthorId)
                continue;

            previousAuthorId = previousUserMessage.Author.Id;
            streak++;
        }

        if (streak > 1)
        {
            await message.Channel.SendMessageAsync(
                $"Streak of {streak} {_emote} broken by {message.Author.Mention}, shame on them."
            );
        }
    }

    public override string ToString() =>
        $"{nameof(EmoteStreakMessageHandler)} - :{_emoteName}: in #{_channelId}";
}
