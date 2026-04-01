using DevOpsAiHub.Application.Common.Exceptions;
using DevOpsAiHub.Application.Common.Interfaces.Auth;
using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.App.Posts.DTOs;
using DevOpsAiHub.Domain.Entities.Posts;

namespace DevOpsAiHub.Application.Features.App.Bookmarks.Services;

public class BookmarkAppService : IBookmarkAppService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IBookmarkRepository _bookmarkRepository;
    private readonly IPostRepository _postRepository;
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;

    public BookmarkAppService(
        ICurrentUserService currentUserService,
        IBookmarkRepository bookmarkRepository,
        IPostRepository postRepository,
        IApplicationDbContext context,
        IDateTimeService dateTimeService)
    {
        _currentUserService = currentUserService;
        _bookmarkRepository = bookmarkRepository;
        _postRepository = postRepository;
        _context = context;
        _dateTimeService = dateTimeService;
    }

    public async Task BookmarkAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post is null)
            throw new NotFoundException("Post not found.");

        if (post.Status == "Deleted" || post.DeletedAt is not null)
            throw new BadRequestException("Post is not available.");

        var alreadyBookmarked = await _bookmarkRepository.ExistsAsync(postId, currentUserId.Value, cancellationToken);
        if (alreadyBookmarked)
            throw new BadRequestException("You already bookmarked this post.");

        var bookmark = new Bookmark
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            UserId = currentUserId.Value,
            CreatedAt = _dateTimeService.UtcNow
        };

        await _bookmarkRepository.AddAsync(bookmark, cancellationToken);

        post.BookmarkCount += 1;
        post.UpdatedAt = _dateTimeService.UtcNow;
        _postRepository.Update(post);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UnbookmarkAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post is null)
            throw new NotFoundException("Post not found.");

        var bookmark = await _bookmarkRepository.GetByPostAndUserAsync(postId, currentUserId.Value, cancellationToken);
        if (bookmark is null)
            throw new NotFoundException("Bookmark not found.");

        _bookmarkRepository.Remove(bookmark);

        if (post.BookmarkCount > 0)
            post.BookmarkCount -= 1;

        post.UpdatedAt = _dateTimeService.UtcNow;
        _postRepository.Update(post);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<PostDto>> GetMyBookmarksAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var bookmarks = await _bookmarkRepository.GetByUserIdAsync(currentUserId.Value, cancellationToken);

        return bookmarks.Select(x => new PostDto
        {
            Id = x.Post.Id,
            AuthorId = x.Post.AuthorId,
            AuthorUsername = x.Post.Author.Username,
            PostType = x.Post.PostType,
            Title = x.Post.Title,
            Slug = x.Post.Slug,
            Summary = x.Post.Summary,
            Status = x.Post.Status,
            Visibility = x.Post.Visibility,
            ViewCount = x.Post.ViewCount,
            LikeCount = x.Post.LikeCount,
            CommentCount = x.Post.CommentCount,
            BookmarkCount = x.Post.BookmarkCount,
            CreatedAt = x.Post.CreatedAt
        }).ToList();
    }
}