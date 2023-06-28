using ChampionsOfKhazad.Bot.ChatBot;
using Discord;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;

namespace ChampionsOfKhazad.Bot;

public class MentionHandler : IMessageReceivedEventHandler
{
    private readonly Assistant _assistant;
    private ulong _botId;

    public MentionHandler(Assistant assistant)
    {
        _assistant = assistant;
    }

    public Task StartAsync(BotContext context)
    {
        _botId = context.BotId;
        return Task.CompletedTask;
    }

    public Task HandleMessageAsync(IUserMessage message)
    {
        if (
            message.Channel is not ITextChannel textChannel
            || !message.MentionedUserIds.Contains(_botId)
        )
            return Task.CompletedTask;

        Task.Run(async () =>
        {
            using var typing = textChannel.EnterTypingState();

            var user = new User { Id = message.Author.Id, Name = message.Author.GlobalName };

            var previousMessages = await message
                .GetPreviousMessagesAsync()
                .Take(20)
                .Reverse()
                .Select(
                    x =>
                        new ChatMessage(
                            x.Author.Id == _botId
                                ? StaticValues.ChatMessageRoles.Assistant
                                : StaticValues.ChatMessageRoles.User,
                            x.CleanContent,
                            x.Author.GlobalName
                        )
                )
                .ToListAsync();

            var response = await _assistant.RespondAsync(
                message.CleanContent,
                user,
                previousMessages
            );

            await message.ReplyAsync(response);
        });

        return Task.CompletedTask;
    }

    public override string ToString() => $"{nameof(MentionHandler)}";
}
