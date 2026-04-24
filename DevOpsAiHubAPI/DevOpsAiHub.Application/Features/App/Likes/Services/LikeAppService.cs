using DevOpsAiHub.Application.Common.Exceptions;
using DevOpsAiHub.Application.Common.Interfaces.Auth;
using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.App.Posts.DTOs;
using DevOpsAiHub.Domain.Entities.Posts;

namespace DevOpsAiHub.Application.Features.App.Likes.Services;

public class LikeAppService : ILikeAppService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILikeRepository _likeRepository;
    private readonly IPostRepository _postRepository;
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;

    public LikeAppService(
        ICurrentUserService currentUserService,
        ILikeRepository likeRepository,
        IPostRepository postRepository,
        IApplicationDbContext context,
        IDateTimeService dateTimeService)
    {
        _currentUserService = currentUserService;
        _likeRepository = likeRepository;
        _postRepository = postRepository;
        _context = context;
        _dateTimeService = dateTimeService;
    }

    public async Task LikeAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post is null)
            throw new NotFoundException("Post not found.");

        if (post.Status == "Deleted" || post.DeletedAt is not null)
            throw new BadRequestException("Post is not available.");

        var alreadyLiked = await _likeRepository.ExistsAsync(postId, currentUserId.Value, cancellationToken);
        if (alreadyLiked)
            throw new BadRequestException("You already liked this post.");

        var like = new Like
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            UserId = currentUserId.Value,
            CreatedAt = _dateTimeService.UtcNow
        };

        await _likeRepository.AddAsync(like, cancellationToken);

        post.LikeCount += 1;
        post.UpdatedAt = _dateTimeService.UtcNow;
        _postRepository.Update(post);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UnlikeAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post is null)
            throw new NotFoundException("Post not found.");

        var like = await _likeRepository.GetByPostAndUserAsync(postId, currentUserId.Value, cancellationToken);
        if (like is null)
            throw new NotFoundException("Like not found.");

        _likeRepository.Remove(like);

        if (post.LikeCount > 0)
            post.LikeCount -= 1;

        post.UpdatedAt = _dateTimeService.UtcNow;
        _postRepository.Update(post);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<PostDto>> GetMyLikedPostsAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var likes = await _likeRepository.GetByUserIdAsync(currentUserId.Value, cancellationToken);

        return likes.Select(x => new PostDto
        {
            Id = x.Post.Id,
            AuthorId = x.Post.AuthorId,
            AuthorUsername = x.Post.Author.Username,
            PostType = x.Post.PostType,
            Title = x.Post.Title,
            Slug = x.Post.Slug,
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