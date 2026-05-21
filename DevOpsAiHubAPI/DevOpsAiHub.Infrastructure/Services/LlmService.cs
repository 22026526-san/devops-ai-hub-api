using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.AI.DTOs;
using Microsoft.Extensions.AI;

namespace DevOpsAiHub.Infrastructure.Services;

public class LlmService : ILlmService
{
    private readonly IChatClient _chatClient;

    public LlmService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> ChatWithHistoryAsync(
        string systemPrompt,
        IEnumerable<ChatMessageDto> history,
        string userMessage,
        CancellationToken ct = default)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt)
        };

        foreach (var h in history)
        {
            var role = h.Role == "user" ? ChatRole.User : ChatRole.Assistant;
            messages.Add(new ChatMessage(role, h.Content));
        }

        messages.Add(new ChatMessage(ChatRole.User, userMessage));

        var options = new ChatOptions
        {
            Temperature = 0.3f,
            TopP = 0.1f,
            AdditionalProperties = new AdditionalPropertiesDictionary
            {
                ["num_ctx"] = 4096,
            }
        };

        var result = await _chatClient.GetResponseAsync(messages, options, ct);
        return result.Text ?? string.Empty;
        
    }
}