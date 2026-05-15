using DevOpsAiHub.Domain.Entities.AI;

namespace DevOpsAiHub.Application.Common.Interfaces.Repositories;

public interface IAiMessageRepository
{
    Task<IEnumerable<AiMessage>> GetByConversationIdAsync(Guid conversationId, CancellationToken ct = default);
    void Add(AiMessage message);
}