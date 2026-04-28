using DevOpsAiHub.Application.Common.Exceptions;
using DevOpsAiHub.Application.Common.Interfaces.Auth;
using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Common.Models;
using DevOpsAiHub.Application.Features.App.Bookmarks.DTOs;
using DevOpsAiHub.Application.Features.App.Posts.DTOs;
using DevOpsAiHub.Domain.Entities.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

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

    public async Task<PagedResult<PostDto>> GetMyBookmarksAsync(GetBookmarksQueryDto request,CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = _bookmarkRepository.GetByUserIdAsync(currentUserId.Value);
        
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

        var bookmarks = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

        var postIds = bookmarks.Select(b => b.PostId).ToList();

        var likedPostIds = new HashSet<Guid>();
        if (postIds.Any())
        {
            var likedList = await _context.Likes
                .Where(l => l.UserId == currentUserId && postIds.Contains(l.PostId))
                .Select(l => l.PostId)
                .ToListAsync(cancellationToken);

            likedPostIds = new HashSet<Guid>(likedList);
        }

        var items = bookmarks.Select(bookmark =>
        {
            var dto = MapToPostDto(bookmark.Post);
            dto.IsLiked = likedPostIds.Contains(bookmark.PostId);
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