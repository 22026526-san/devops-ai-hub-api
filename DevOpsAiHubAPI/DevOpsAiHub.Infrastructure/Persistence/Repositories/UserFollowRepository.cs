using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Domain.Entities.Users;
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

    public async Task<int> CountFollowingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserFollows
            .CountAsync(x => x.FollowerId == userId, cancellationToken);
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

    public async Task<Dictionary<Guid, int>> CountFollowingByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var userIdList = userIds.ToList();

        return await _context.UserFollows
            .Where(x => userIdList.Contains(x.FollowerId)) 
            .GroupBy(x => x.FollowerId)               
            .Select(g => new
            {
                UserId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellationToken);
    }

    public async Task<List<Guid>> GetFollowingUserIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserFollows
            .Where(x => x.FollowerId == userId)
            .Select(x => x.FollowingId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default)
    {
        return await _context.UserFollows
            .AnyAsync(x => x.FollowerId == followerId && x.FollowingId == followingId, cancellationToken);
    }

    public async Task AddAsync(UserFollow userFollow, CancellationToken cancellationToken = default)
    {
        await _context.UserFollows.AddAsync(userFollow, cancellationToken);
    }

    public async Task<UserFollow?> GetByFollowerAndFollowingAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default)
    {
        return await _context.UserFollows
            .FirstOrDefaultAsync(x => x.FollowerId == followerId && x.FollowingId == followingId, cancellationToken);
    }

    public void Remove(UserFollow userFollow)
    {
        _context.UserFollows.Remove(userFollow);
    }

    public async Task<List<UserFollow>> GetFollowersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserFollows
            .Include(x => x.Follower)
                .ThenInclude(x => x.Profile)
            .Where(x => x.FollowingId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UserFollow>> GetFollowingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserFollows
            .Include(x => x.Following)
                .ThenInclude(x => x.Profile)
            .Where(x => x.FollowerId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}