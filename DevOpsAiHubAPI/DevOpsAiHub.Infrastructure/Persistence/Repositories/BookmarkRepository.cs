using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Domain.Entities.Posts;
using Microsoft.EntityFrameworkCore;

namespace DevOpsAiHub.Infrastructure.Persistence.Repositories;

public class BookmarkRepository : IBookmarkRepository
{
    private readonly IApplicationDbContext _context;

    public BookmarkRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Bookmark?> GetByPostAndUserAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookmarks
            .FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == userId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookmarks
            .AnyAsync(x => x.PostId == postId && x.UserId == userId, cancellationToken);
    }

    public IQueryable<Bookmark> GetByUserIdAsync(Guid userId)
    {
        return _context.Bookmarks
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

    public async Task AddAsync(Bookmark bookmark, CancellationToken cancellationToken = default)
    {
        await _context.Bookmarks.AddAsync(bookmark, cancellationToken);
    }

    public void Remove(Bookmark bookmark)
    {
        _context.Bookmarks.Remove(bookmark);
    }
}