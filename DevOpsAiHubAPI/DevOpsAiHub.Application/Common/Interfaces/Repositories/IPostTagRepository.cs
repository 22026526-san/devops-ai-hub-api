using DevOpsAiHub.Domain.Entities.Posts;

namespace DevOpsAiHub.Application.Common.Interfaces.Repositories;

public interface IPostTagRepository
{
    Task<List<PostTag>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task AddRangeAsync(List<PostTag> postTags, CancellationToken cancellationToken = default);
    void RemoveRange(List<PostTag> postTags);
}