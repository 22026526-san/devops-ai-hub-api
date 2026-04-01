using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Domain.Entities.Posts;
using Microsoft.EntityFrameworkCore;

namespace DevOpsAiHub.Infrastructure.Persistence.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly IApplicationDbContext _context;

    public CommentRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Comment>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.Comments
            .Include(x => x.Author)
                .ThenInclude(x => x.Profile)
            .Where(x => x.PostId == postId && x.DeletedAt == null)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Comments
            .Include(x => x.Author)
                .ThenInclude(x => x.Profile)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
    }

    public async Task AddAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        await _context.Comments.AddAsync(comment, cancellationToken);
    }

    public void Update(Comment comment)
    {
        _context.Comments.Update(comment);
    }
}