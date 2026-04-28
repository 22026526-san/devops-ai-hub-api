using DevOpsAiHub.Domain.Entities.Posts;

namespace DevOpsAiHub.Application.Common.Interfaces.Repositories;

public interface IBookmarkRepository
{
    Task<Bookmark?> GetByPostAndUserAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
    IQueryable<Bookmark> GetByUserIdAsync(Guid userId);
    Task AddAsync(Bookmark bookmark, CancellationToken cancellationToken = default);
    void Remove(Bookmark bookmark);
}