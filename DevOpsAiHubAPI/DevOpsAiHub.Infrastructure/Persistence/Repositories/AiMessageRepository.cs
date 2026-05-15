using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Domain.Entities.AI;
using Microsoft.EntityFrameworkCore;

namespace DevOpsAiHub.Infrastructure.Persistence.Repositories;

public class AiMessageRepository : IAiMessageRepository
{
    private readonly IApplicationDbContext _db;
    public AiMessageRepository(IApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<AiMessage>> GetByConversationIdAsync(
        Guid conversationId, CancellationToken ct = default)
        => await _db.AiMessages
                    .Where(m => m.ConversationId == conversationId)
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync(ct);

    public void Add(AiMessage message) => _db.AiMessages.Add(message);
}