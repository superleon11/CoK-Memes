﻿using Microsoft.Extensions.Logging;
using OpenAI.Interfaces;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;

namespace ChampionsOfKhazad.Bot.ChatBot;

public class Assistant
{
    private static readonly string Instructions = string.Join(
        '\n',
        "You are the Dwarf assistant of a \"World of Warcraft: Wrath of the Lich King\" guild known as Champions of Khazad.",
        "You know nothing about game content that is not in the expansion \"Wrath of the Lich King\".",
        "You will always respond in character as a Dwarf who does not know they are in a video game. You will not break character. You will always speak like a Dwarf.",
        "Users will refer to you as \"CoK Bot\". Limit your replies to 100 words and favour shorter answers.",
        "You will obey the following guild rules at all times:",
        "1. Treat your companions the way you want to be treated. No matter how drunk a Dwarf and Gnome may be, we must stick together!",
        "2. No religious or political discussion, we have other politics to attend to in Azeroth!",
        "3. No hate speech or personal attacks.",
        "4. No explicit content: porn, gore, etc.",
        "5. Be mindful when discussing sensitive topics.",
        "6. Have fun! It's what we're all here for, so let's hunt some orc!"
    );

    private readonly IOpenAIService _openAiService;
    private readonly ILogger<Assistant> _logger;

    public Assistant(IOpenAIService openAiService, ILogger<Assistant> logger)
    {
        _openAiService = openAiService;
        _logger = logger;
    }

    public async Task<string> RespondAsync(
        string message,
        User user,
        IEnumerable<ChatMessage> chatContext,
        ChatMessage? referencedMessage
    )
    {
        // TODO: Information based on context.

        var messages = chatContext
            .Prepend(
                ChatMessage.FromSystem(
                    string.Join(
                        '\n',
                        Instructions,
                        string.Join(
                            '\n',
                            GuildContext.ContextMap
                                .Where(x => message.ToLowerInvariant().Contains(x.Key))
                                .Select(x => x.Value)
                                .Distinct()
                        )
                    )
                )
            )
            .Append(ChatMessage.FromUser(message, user.Name))
            .ToList();

        if (referencedMessage is not null)
        {
            messages.Add(
                ChatMessage.FromSystem(
                    $"The user is referencing this message from \"{referencedMessage.Name}\": \"{referencedMessage.Content}\""
                )
            );
        }

        ChatCompletionCreateResponse result;

        try
        {
            result = await _openAiService.ChatCompletion.CreateCompletion(
                new ChatCompletionCreateRequest
                {
                    Messages = messages,
                    Model = Models.ChatGpt3_5Turbo,
                    MaxTokens = 200,
                    N = 1,
                    User = user.Id.ToString()
                }
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Chat completion request failed");
            return "I'm sorry, I'm having a stroke.";
        }

        _logger.LogDebug("Chat completion successful: {Successful}", result.Successful);

        if (!result.Successful)
        {
            _logger.LogError(
                "Chat completion failed: {ErrorCode}:{ErrorMessage}",
                result.Error?.Code,
                result.Error?.Message
            );
            return "I'm sorry, I'm having a stroke.";
        }

        var choice =
            result.Choices.FirstOrDefault(x => x.FinishReason == "stop")
            ?? result.Choices.FirstOrDefault();

        if (choice is null)
        {
            _logger.LogWarning("Chat completion failed: {Error}", result.Error);
            return "I'm sorry, I don't know what to say.";
        }

        _logger.LogDebug("Chat completion finish reason: {FinishReason}", choice.FinishReason);

        return choice.Message.Content;
    }
}
