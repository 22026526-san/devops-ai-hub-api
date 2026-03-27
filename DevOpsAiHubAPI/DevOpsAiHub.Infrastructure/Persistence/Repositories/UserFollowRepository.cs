using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DevOpsAiHub.Infrastructure.Persistence.Repositories;

public class UserFollowRepository : IUserFollowRepository
{
    private readonly IApplicationDbContext _context;

    public UserFollowRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> CountFollowersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserFollows
            .CountAsync(x => x.FollowingId == userId, cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> CountFollowersByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var userIdList = userIds.ToList();

        return await _context.UserFollows
            .Where(x => userIdList.Contains(x.FollowingId))
            .GroupBy(x => x.FollowingId)
            .Select(g => new
            {
                UserId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellationToken);
    }
}