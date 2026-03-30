using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Domain.Entities.Posts;
using Microsoft.EntityFrameworkCore;

namespace DevOpsAiHub.Infrastructure.Persistence.Repositories;

public class PipelineRepository : IPipelineRepository
{
    private readonly IApplicationDbContext _context;

    public PipelineRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Post?> GetPipelinePostByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.Posts
            .Include(x => x.Author)
            .Include(x => x.PipelinePost)
                .ThenInclude(x => x!.CurrentVersion)
            .FirstOrDefaultAsync(x =>
                x.Id == postId &&
                x.PostType == "Pipeline" &&
                x.DeletedAt == null,
                cancellationToken);
    }

    public async Task<List<PipelineVersion>> GetVersionsByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.PipelineVersions
            .Include(x => x.Creator)
            .Where(x => x.PipelinePostId == postId)
            .OrderByDescending(x => x.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<PipelineVersion?> GetVersionByIdAsync(Guid versionId, CancellationToken cancellationToken = default)
    {
        return await _context.PipelineVersions
            .Include(x => x.Creator)
            .FirstOrDefaultAsync(x => x.Id == versionId, cancellationToken);
    }

    public async Task<int> GetNextVersionNumberAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var currentMax = await _context.PipelineVersions
            .Where(x => x.PipelinePostId == postId)
            .Select(x => (int?)x.VersionNumber)
            .MaxAsync(cancellationToken);

        return (currentMax ?? 0) + 1;
    }

    public async Task AddVersionAsync(PipelineVersion version, CancellationToken cancellationToken = default)
    {
        await _context.PipelineVersions.AddAsync(version, cancellationToken);
    }
}