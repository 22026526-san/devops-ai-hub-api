using DevOpsAiHub.Domain.Entities.AI;

namespace DevOpsAiHub.Application.Common.Interfaces.Repositories;

public interface IAiConversationRepository
{
    Task<List<AiConversation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AiConversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(AiConversation conversation, CancellationToken cancellationToken = default);
    void Update(AiConversation conversation);
}