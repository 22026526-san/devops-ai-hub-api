using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Domain.Entities.AI;
using Microsoft.EntityFrameworkCore;

namespace DevOpsAiHub.Infrastructure.Persistence.Repositories;

public class AiConversationRepository : IAiConversationRepository
{
    private readonly IApplicationDbContext _db;
    public AiConversationRepository(IApplicationDbContext db) => _db = db;

    public Task<AiConversation?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.AiConversations
              .Include(c => c.Messages)
              .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IEnumerable<AiConversation>> GetByUserIdAsync(
        Guid userId, CancellationToken ct = default)
        => await _db.AiConversations
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.UpdatedAt)
                    .ToListAsync(ct);

    public async Task<AiConversation> CreateAsync(
        AiConversation conv, CancellationToken ct = default)
    {
        _db.AiConversations.Add(conv);
        await _db.SaveChangesAsync(ct);
        return conv;
    }

    public async Task DeleteAsync(Guid conversationId, CancellationToken ct = default)
    {
        var conv = await _db.AiConversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == conversationId, ct);

        if (conv is null) return;

        _db.AiMessages.RemoveRange(conv.Messages);
        _db.AiConversations.Remove(conv);
    }

    public async Task UpdateTitleAsync(
        Guid conversationId, string newTitle, CancellationToken ct = default)
    {
        var conv = await _db.AiConversations
            .FirstOrDefaultAsync(c => c.Id == conversationId, ct);

        if (conv is null) return;

        conv.Title = newTitle;
        conv.UpdatedAt = DateTime.UtcNow;
        _db.AiConversations.Update(conv);
    }

    public void Update(AiConversation conv)
        => _db.AiConversations.Update(conv);
}