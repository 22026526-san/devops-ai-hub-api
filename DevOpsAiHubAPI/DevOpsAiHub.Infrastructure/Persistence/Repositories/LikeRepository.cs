using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Domain.Entities.Posts;
using Microsoft.EntityFrameworkCore;

namespace DevOpsAiHub.Infrastructure.Persistence.Repositories;

public class LikeRepository : ILikeRepository
{
    private readonly IApplicationDbContext _context;

    public LikeRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Like?> GetByPostAndUserAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Likes
            .FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == userId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Likes
            .AnyAsync(x => x.PostId == postId && x.UserId == userId, cancellationToken);
    }
    public IQueryable<Like> GetByUserIdAsync(Guid userId)
    {
        return _context.Likes
            .Include(x => x.Post)
                .ThenInclude(p => p.Author)
                    .ThenInclude(a => a.Profile)

            .Include(x => x.Post)
                .ThenInclude(p => p.QuestionPost)

            .Include(x => x.Post)
                .ThenInclude(p => p.PipelinePost)
                    .ThenInclude(pl => pl!.CurrentVersion)

            .Include(x => x.Post)
                .ThenInclude(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
            .Where(x => x.UserId == userId && x.Post.DeletedAt == null)
            .OrderByDescending(x => x.CreatedAt);
    }

    public async Task AddAsync(Like like, CancellationToken cancellationToken = default)
    {
        await _context.Likes.AddAsync(like, cancellationToken);
    }

    public void Remove(Like like)
    {
        _context.Likes.Remove(like);
    }
}