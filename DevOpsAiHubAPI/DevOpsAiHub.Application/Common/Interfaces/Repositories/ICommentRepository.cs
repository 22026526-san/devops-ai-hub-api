using DevOpsAiHub.Domain.Entities.Posts;

namespace DevOpsAiHub.Application.Common.Interfaces.Repositories;

public interface ICommentRepository
{
    Task<List<Comment>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Comment comment, CancellationToken cancellationToken = default);
    void Update(Comment comment);
}