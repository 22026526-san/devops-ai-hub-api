using DevOpsAiHub.Domain.Entities.Posts;

namespace DevOpsAiHub.Application.Common.Interfaces.Repositories;

public interface IPipelineRepository
{
    Task<Post?> GetPipelinePostByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<List<PipelineVersion>> GetVersionsByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<PipelineVersion?> GetVersionByIdAsync(Guid versionId, CancellationToken cancellationToken = default);
    Task<int> GetNextVersionNumberAsync(Guid postId, CancellationToken cancellationToken = default);
    Task AddVersionAsync(PipelineVersion version, CancellationToken cancellationToken = default);
}