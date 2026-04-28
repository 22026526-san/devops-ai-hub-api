using DevOpsAiHub.Application.Common.Exceptions;
using DevOpsAiHub.Application.Common.Interfaces.Auth;
using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Common.Models;
using DevOpsAiHub.Application.Features.App.Likes.DTOs;
using DevOpsAiHub.Application.Features.App.Posts.DTOs;
using DevOpsAiHub.Domain.Entities.Posts;
using Microsoft.EntityFrameworkCore;

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

    public async Task<PagedResult<PostDto>> GetMyLikedPostsAsync(GetLikesQueryDto request,CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = _likeRepository.GetByUserIdAsync(currentUserId.Value);

        if (request.TagIds.Any())
        {
            query = query.Where(x => x.Post.PostTags.Any(pt => request.TagIds.Contains(pt.TagId)));
        }

        if (request.Year.HasValue)
        {
            query = query.Where(x => x.CreatedAt.Year == request.Year.Value);
        }

        if (request.Month.HasValue)
        {
            query = query.Where(x => x.CreatedAt.Month == request.Month.Value);
        }

        if (request.Day.HasValue)
        {
            query = query.Where(x => x.CreatedAt.Day == request.Day.Value);
        }

        query = request.SortBy switch
        {
            "likes_desc" => query.OrderByDescending(x => x.Post.LikeCount).ThenByDescending(x => x.CreatedAt),
            "views_desc" => query.OrderByDescending(x => x.Post.ViewCount).ThenByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };
        var totalItems = await query.CountAsync(cancellationToken);

        var likes = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

        var postIds = likes.Select(b => b.PostId).ToList();

        var likedPostIds = new HashSet<Guid>();
        if (postIds.Any())
        {
            var likedList = await _context.Likes
                .Where(l => l.UserId == currentUserId && postIds.Contains(l.PostId))
                .Select(l => l.PostId)
                .ToListAsync(cancellationToken);

            likedPostIds = new HashSet<Guid>(likedList);
        }

        var items = likes.Select(like =>
        {
            var dto = MapToPostDto(like.Post);
            dto.IsLiked = likedPostIds.Contains(like.PostId);
            dto.IsBookmarked = true;

            return dto;
        }).ToList();

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResult<PostDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasNextPage = page < totalPages
        };
    }

    private static PostDto MapToPostDto(Post post)
    {
        object? detail = null;

        if (post.PostType == "Question" && post.QuestionPost is not null)
        {
            detail = new QuestionPostDetailDto
            {
                Content = post.QuestionPost.Content,
                ImgUrl = post.QuestionPost.ImgUrl
            };
        }
        else if (post.PostType == "Pipeline" && post.PipelinePost is not null)
        {
            detail = new PipelinePostDetailDto
            {
                Description = post.PipelinePost.Description,
                VersionCount = post.PipelinePost.VersionCount,
                PipelineContent = post.PipelinePost.CurrentVersion?.Content
            };
        }

        return new PostDto
        {
            Id = post.Id,
            AuthorId = post.AuthorId,
            AuthorUsername = post.Author?.Profile?.FullName,
            AuthorImage = post.Author?.Profile?.AvatarUrl,
            PostType = post.PostType,
            Title = post.Title,
            Slug = post.Slug,
            Status = post.Status,
            Visibility = post.Visibility,
            ViewCount = post.ViewCount,
            LikeCount = post.LikeCount,
            CommentCount = post.CommentCount,
            BookmarkCount = post.BookmarkCount,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            Tags = post.PostTags
                .Select(pt => new PostTagDto
                {
                    Id = pt.Tag.Id,
                    Name = pt.Tag.Name
                })
                .ToList(),
            Detail = detail
        };
    }
}