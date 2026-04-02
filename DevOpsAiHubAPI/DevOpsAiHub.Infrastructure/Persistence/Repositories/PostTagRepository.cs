using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Domain.Entities.Posts;
using Microsoft.EntityFrameworkCore;

namespace DevOpsAiHub.Infrastructure.Persistence.Repositories;

public class PostTagRepository : IPostTagRepository
{
    private readonly IApplicationDbContext _context;

    public PostTagRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PostTag>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.PostTags
            .Where(x => x.PostId == postId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(List<PostTag> postTags, CancellationToken cancellationToken = default)
    {
        await _context.PostTags.AddRangeAsync(postTags, cancellationToken);
    }

    public void RemoveRange(List<PostTag> postTags)
    {
        _context.PostTags.RemoveRange(postTags);
    }
}