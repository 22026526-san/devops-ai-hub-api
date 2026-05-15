using DevOpsAiHub.Domain.Entities.AI;

namespace DevOpsAiHub.Application.Common.Interfaces.Repositories;

public interface IAiConversationRepository
{
    Task<AiConversation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<AiConversation>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<AiConversation> CreateAsync(AiConversation conversation, CancellationToken ct = default);
    Task DeleteAsync(Guid conversationId, CancellationToken ct = default);
    Task UpdateTitleAsync(Guid conversationId, string newTitle, CancellationToken ct = default);
    void Update(AiConversation conversation);
}