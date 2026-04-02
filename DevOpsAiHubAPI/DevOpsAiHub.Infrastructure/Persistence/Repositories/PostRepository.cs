using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Domain.Entities.Posts;
using Microsoft.EntityFrameworkCore;

namespace DevOpsAiHub.Infrastructure.Persistence.Repositories;

public class PostRepository : IPostRepository
{
    private readonly IApplicationDbContext _context;

    public PostRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public IQueryable<Post> Query()
    {
        return _context.Posts
            .Include(x => x.Author)
            .Include(x => x.QuestionPost)
            .Include(x => x.PipelinePost)
                .ThenInclude(x => x!.CurrentVersion)
            .Include(x => x.PostTags)
                .ThenInclude(x => x.Tag)
            .Where(x => x.DeletedAt == null && x.Status == "Published");
    }

    public async Task<List<Post>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Query()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Posts
            .Include(x => x.Author)
            .Include(x => x.QuestionPost)
            .Include(x => x.PipelinePost)
                .ThenInclude(x => x!.CurrentVersion)
            .Include(x => x.PostTags)
                .ThenInclude(x => x.Tag)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
    }

    public async Task<Post?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Posts
            .Include(x => x.Author)
            .Include(x => x.QuestionPost)
            .Include(x => x.PipelinePost)
                .ThenInclude(x => x!.CurrentVersion)
            .Include(x => x.PostTags)
                .ThenInclude(x => x.Tag)
            .FirstOrDefaultAsync(x => x.Slug == slug && x.DeletedAt == null, cancellationToken);
    }

    public async Task AddAsync(Post post, CancellationToken cancellationToken = default)
    {
        await _context.Posts.AddAsync(post, cancellationToken);
    }

    public void Update(Post post)
    {
        _context.Posts.Update(post);
    }
}