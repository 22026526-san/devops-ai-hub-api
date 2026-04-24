using DevOpsAiHub.Domain.Entities.AI;

namespace DevOpsAiHub.Application.Common.Interfaces.Repositories;

public interface IAiMessageRepository
{
    Task<List<AiMessage>> GetByConversationIdAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task AddRangeAsync(List<AiMessage> messages, CancellationToken cancellationToken = default);
}