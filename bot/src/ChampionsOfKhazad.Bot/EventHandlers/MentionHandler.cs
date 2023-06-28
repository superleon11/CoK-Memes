﻿using System.Text.RegularExpressions;
using ChampionsOfKhazad.Bot.ChatBot;
using Discord;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;

namespace ChampionsOfKhazad.Bot;

public class MentionHandler : IMessageReceivedEventHandler
{
    private static readonly Regex NameExpression =
        new("^[a-zA-Z0-9_-]{1,64}$", RegexOptions.Compiled);
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

            var user = new User { Id = message.Author.Id, Name = GetFriendlyAuthorName(message) };

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
                            GetFriendlyAuthorName(x)
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

    private static string GetFriendlyAuthorName(IMessage message) =>
        message.Author is IGuildUser guildUser && NameExpression.IsMatch(guildUser.DisplayName)
            ? guildUser.DisplayName
            : NameExpression.IsMatch(message.Author.GlobalName)
                ? message.Author.GlobalName
                : NameExpression.IsMatch(message.Author.Username)
                    ? message.Author.Username
                    : message.Author.Id.ToString();

    public override string ToString() => $"{nameof(MentionHandler)}";
}
