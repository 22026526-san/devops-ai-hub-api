using DevOpsAiHub.Application.Features.App.Follows.DTOs;

namespace DevOpsAiHub.Application.Features.Follows.Services;

public interface IFollowAppService
{
    Task FollowUserAsync(Guid targetUserId, CancellationToken cancellationToken = default);
    Task UnfollowUserAsync(Guid targetUserId, CancellationToken cancellationToken = default);
    Task<List<UserFollowDto>> GetFollowersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<UserFollowDto>> GetFollowingAsync(Guid userId, CancellationToken cancellationToken = default);
}