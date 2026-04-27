using DevOpsAiHub.Domain.Entities.Users;

namespace DevOpsAiHub.Application.Common.Interfaces.Repositories;

public interface IUserFollowRepository
{
    Task<int> CountFollowersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountFollowingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> CountFollowersByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> CountFollowingByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default);
    Task AddAsync(UserFollow userFollow, CancellationToken cancellationToken = default);
    Task<UserFollow?> GetByFollowerAndFollowingAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default);
    void Remove(UserFollow userFollow);

    Task<List<UserFollow>> GetFollowersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<UserFollow>> GetFollowingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetFollowingUserIdsAsync(Guid userId, CancellationToken cancellationToken = default);
}