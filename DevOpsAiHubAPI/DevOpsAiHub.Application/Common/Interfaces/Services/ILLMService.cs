using DevOpsAiHub.Application.Features.AI.DTOs;

namespace DevOpsAiHub.Application.Common.Interfaces.Services;

public interface ILlmService
{
    Task<string> ChatWithHistoryAsync(
        string systemPrompt,
        IEnumerable<ChatMessageDto> history,
        string userMessage,
        CancellationToken ct = default);
}