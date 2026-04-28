using DevOpsAiHub.Domain.Entities.Posts;

namespace DevOpsAiHub.Application.Common.Interfaces.Repositories;

public interface ILikeRepository
{
    Task<Like?> GetByPostAndUserAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
    IQueryable<Like> GetByUserIdAsync(Guid userId);
    Task AddAsync(Like like, CancellationToken cancellationToken = default);
    void Remove(Like like);
}