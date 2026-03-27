namespace DevOpsAiHub.Application.Common.Interfaces.Repositories;

public interface IUserFollowRepository
{
    Task<int> CountFollowersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> CountFollowersByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
}