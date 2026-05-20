using DevOpsAiHub.Application.Common.Exceptions;
using DevOpsAiHub.Application.Common.Interfaces.Auth;
using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Common.Models;
using DevOpsAiHub.Application.Features.App.Posts.DTOs;
using DevOpsAiHub.Domain.Entities.Posts;
using DevOpsAiHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DevOpsAiHub.Application.Features.App.Posts.Services;

public class PostAppService : IPostAppService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPostRepository _postRepository;
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly ISlugService _slugService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ITagRepository _tagRepository;
    private readonly IPostTagRepository _postTagRepository;

    public PostAppService(
    ICurrentUserService currentUserService,
    IPostRepository postRepository,
    IApplicationDbContext context,
    IDateTimeService dateTimeService,
    ISlugService slugService,
    ICloudinaryService cloudinaryService,
    ITagRepository tagRepository,
    IPostTagRepository postTagRepository)
    {
        _currentUserService = currentUserService;
        _postRepository = postRepository;
        _context = context;
        _dateTimeService = dateTimeService;
        _slugService = slugService;
        _cloudinaryService = cloudinaryService;
        _tagRepository = tagRepository;
        _postTagRepository = postTagRepository;
    }
    public async Task<PagedResult<PostDto>> GetAllAsync(GetPostsQueryDto request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = _postRepository.Query();

        query = query.Where(x => x.Visibility == PostVisibility.Public || (request.CurrentUserId != null && x.AuthorId == request.CurrentUserId) ||
             (request.CurrentUserId != null && x.Visibility == PostVisibility.Followers &&
            _context.UserFollows.Any(f => f.FollowerId == request.CurrentUserId && f.FollowingId == x.AuthorId))
        );

        if (request.FilterUserId.HasValue)
        {
            query = query.Where(x => x.AuthorId == request.FilterUserId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(x => x.Title.ToLower().Contains(keyword));
        }

        if (request.TagIds.Any())
        {
            query = query.Where(x => x.PostTags.Any(pt => request.TagIds.Contains(pt.TagId)));
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
            "likes_desc" => query.OrderByDescending(x => x.LikeCount).ThenByDescending(x => x.UpdatedAt),
            "views_desc" => query.OrderByDescending(x => x.ViewCount).ThenByDescending(x => x.UpdatedAt),
            _ => query.OrderByDescending(x => x.UpdatedAt)
        };

        var totalItems = await query.CountAsync(cancellationToken);

        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var postIds = posts.Select(p => p.Id).ToList();
        var currentUserId = request.CurrentUserId; 

        var likedPostIds = new HashSet<Guid>();
        var bookmarkedPostIds = new HashSet<Guid>();

        if (currentUserId.HasValue)
        {
            var likedList = await _context.Likes
                .Where(l => l.UserId == currentUserId && postIds.Contains(l.PostId))
                .Select(l => l.PostId)
                .ToListAsync(cancellationToken);
            likedPostIds = new HashSet<Guid>(likedList);

            var bookmarkedList = await _context.Bookmarks
                .Where(b => b.UserId == currentUserId && postIds.Contains(b.PostId))
                .Select(b => b.PostId)
                .ToListAsync(cancellationToken);
            bookmarkedPostIds = new HashSet<Guid>(bookmarkedList);
        }

        var items = posts.Select(p => {
            var dto = MapToPostDto(p);
            dto.IsLiked = likedPostIds.Contains(p.Id);
            dto.IsBookmarked = bookmarkedPostIds.Contains(p.Id);
            return dto;
        }).ToList();

        return new PagedResult<PostDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            HasNextPage = page * pageSize < totalItems
        };
    }

    public async Task<PostDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(id, cancellationToken);
        if (post is null)
            throw new NotFoundException("Post not found.");

        post.ViewCount += 1;
        post.UpdatedAt = _dateTimeService.UtcNow;
        _postRepository.Update(post);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToPostDto(post);
    }

    public async Task<PostDto> CreateQuestionPostAsync(CreateQuestionPostRequestDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new BadRequestException("Title is required.");

        if (string.IsNullOrWhiteSpace(request.Content) && request.Image is null)
            throw new BadRequestException("Question content or image is required.");

        var now = _dateTimeService.UtcNow;
        var postId = Guid.NewGuid();

        string? imageUrl = null;
        string? imagePublicId = null;

        if (request.Image is not null && request.Image.Length > 0)
        {
            await using var stream = request.Image.OpenReadStream();
            var uploadResult = await _cloudinaryService.UploadImageAsync(stream, request.Image.FileName, cancellationToken);
            imageUrl = uploadResult.Url;
            imagePublicId = uploadResult.PublicId;
        }

        var post = new Post
        {
            Id = postId,
            AuthorId = currentUserId.Value,
            PostType = "Question",
            Title = request.Title.Trim(),
            Slug = await GenerateUniqueSlugAsync(request.Title.Trim(), cancellationToken),
            Status = "Published",
            Visibility = request.Visibility,
            ViewCount = 0,
            LikeCount = 0,
            CommentCount = 0,
            BookmarkCount = 0,
            CreatedAt = now,
            UpdatedAt = now
        };

        var questionPost = new QuestionPost
        {
            PostId = postId,
            Content = request.Content?.Trim() ?? string.Empty,
            ImgUrl = imageUrl,
            ImgPublicId = imagePublicId,
            CreatedAt = now,
            UpdatedAt = now
        };

        var tagEntities = new List<Tag>();

        if (request.TagIds.Any())
        {
            tagEntities = await _tagRepository.GetByIdsAsync(request.TagIds.Distinct().ToList(), cancellationToken);

            if (tagEntities.Count != request.TagIds.Distinct().Count())
                throw new BadRequestException("One or more tags are invalid.");
        }

        await _postRepository.AddAsync(post, cancellationToken);
        await _context.QuestionPosts.AddAsync(questionPost, cancellationToken);

        if (tagEntities.Any())
        {
            var postTags = tagEntities.Select(tag => new PostTag
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                TagId = tag.Id,
                CreatedAt = now
            }).ToList();

            await _postTagRepository.AddRangeAsync(postTags, cancellationToken);

            foreach (var tag in tagEntities)
            {
                tag.PostCount += 1;
                _tagRepository.Update(tag);
            }
        }
        await _context.SaveChangesAsync(cancellationToken);

        var createdPost = await _postRepository.GetByIdAsync(postId, cancellationToken)
            ?? throw new NotFoundException("Created post not found.");

        return MapToPostDto(createdPost);
    }

    public async Task<PostDto> CreatePipelinePostAsync(CreatePipelinePostRequestDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new BadRequestException("Title is required.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new BadRequestException("Pipeline content is required.");

        var now = _dateTimeService.UtcNow;
        var postId = Guid.NewGuid();
        var versionId = Guid.NewGuid();

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {

            var post = new Post
            {
                Id = postId,
                AuthorId = currentUserId.Value,
                PostType = "Pipeline",
                Title = request.Title.Trim(),
                Slug = await GenerateUniqueSlugAsync(request.Title.Trim(), cancellationToken),
                Status = "Published",
                Visibility = request.Visibility,
                ViewCount = 0,
                LikeCount = 0,
                CommentCount = 0,
                BookmarkCount = 0,
                CreatedAt = now,
                UpdatedAt = now
            };

            var pipelinePost = new PipelinePost
            {
                PostId = postId,
                SourcePostId = null,
                CurrentVersionId = null,
                Description = request.Description?.Trim(),
                VersionCount = 1,
                CreatedAt = now,
                UpdatedAt = now
            };

            var version = new PipelineVersion
            {
                Id = versionId,
                PipelinePostId = postId,
                VersionNumber = 1,
                Content = request.Content,
                Changelog = request.Changelog?.Trim(),
                CreatedBy = currentUserId.Value,
                CreatedAt = now
            };

            var tagEntities = new List<Tag>();

            if (request.TagIds.Any())
            {
                tagEntities = await _tagRepository.GetByIdsAsync(request.TagIds.Distinct().ToList(), cancellationToken);

                if (tagEntities.Count != request.TagIds.Distinct().Count())
                    throw new BadRequestException("One or more tags are invalid.");
            }

            await _postRepository.AddAsync(post, cancellationToken);
            await _context.PipelinePosts.AddAsync(pipelinePost, cancellationToken);
            await _context.PipelineVersions.AddAsync(version, cancellationToken);

            if (tagEntities.Any())
            {
                var postTags = tagEntities.Select(tag => new PostTag
                {
                    Id = Guid.NewGuid(),
                    PostId = postId,
                    TagId = tag.Id,
                    CreatedAt = now
                }).ToList();

                await _postTagRepository.AddRangeAsync(postTags, cancellationToken);

                foreach (var tag in tagEntities)
                {
                    tag.PostCount += 1;
                    _tagRepository.Update(tag);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            pipelinePost.CurrentVersionId = versionId;
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            var createdPost = await _postRepository.GetByIdAsync(postId, cancellationToken)
                ?? throw new NotFoundException("Created pipeline post not found.");

            return MapToPostDto(createdPost);
        } catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<PostDto> UpdatePostAsync(Guid id, UpdatePostRequestDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var post = await _postRepository.GetByIdAsync(id, cancellationToken);
        if (post is null)
            throw new NotFoundException("Post not found.");

        if (post.AuthorId != currentUserId.Value)
            throw new ForbiddenAccessException("You are not allowed to update this post.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new BadRequestException("Title is required.");

        post.Title = request.Title.Trim();
        post.Visibility = request.Visibility;
        post.UpdatedAt = _dateTimeService.UtcNow;

        if (post.PostType == "Question")
        {
            if (post.QuestionPost is null)
                throw new BadRequestException("Question content not found.");

            if (string.IsNullOrWhiteSpace(request.QuestionContent) && request.QuestionImage is null)
                throw new BadRequestException("Question content or image is required.");

            string? imageUrl = post.QuestionPost.ImgUrl;
            string? imagePublicId = post.QuestionPost.ImgPublicId;

            if (request.QuestionImage is not null && request.QuestionImage.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(post.QuestionPost.ImgPublicId))
                {
                    await _cloudinaryService.DeleteImageAsync(post.QuestionPost.ImgPublicId, cancellationToken);
                }

                await using var stream = request.QuestionImage.OpenReadStream();
                var uploadResult = await _cloudinaryService.UploadImageAsync(stream, request.QuestionImage.FileName, cancellationToken);
                imageUrl = uploadResult.Url;
                imagePublicId = uploadResult.PublicId;
            }

            post.QuestionPost.Content = request.QuestionContent?.Trim() ?? string.Empty;
            post.QuestionPost.ImgUrl = imageUrl;
            post.QuestionPost.ImgPublicId = imagePublicId;
            post.QuestionPost.UpdatedAt = _dateTimeService.UtcNow;
        }
        else if (post.PostType == "Pipeline")
        {
            if (post.PipelinePost is null)
                throw new BadRequestException("Pipeline post not found.");

            if (string.IsNullOrWhiteSpace(request.PipelineContent))
            {
                throw new BadRequestException("Pipeline data is invalid.");
            }

            post.PipelinePost.Description = request.PipelineDescription?.Trim();
            post.PipelinePost.UpdatedAt = _dateTimeService.UtcNow;

            var nextVersionNumber = post.PipelinePost.VersionCount + 1;
            var newVersionId = Guid.NewGuid();

            var version = new PipelineVersion
            {
                Id = newVersionId,
                PipelinePostId = post.PipelinePost.PostId,
                VersionNumber = nextVersionNumber,
                Content = request.PipelineContent,
                Changelog = request.Changelog?.Trim(),
                CreatedBy = currentUserId.Value,
                CreatedAt = _dateTimeService.UtcNow
            };

            await _context.PipelineVersions.AddAsync(version, cancellationToken);

            post.PipelinePost.CurrentVersionId = newVersionId;
            post.PipelinePost.VersionCount = nextVersionNumber;
        }

        _postRepository.Update(post);

        var existingPostTags = await _postTagRepository.GetByPostIdAsync(post.Id, cancellationToken);
        var existingTagIds = existingPostTags.Select(x => x.TagId).ToHashSet();
        var newTagIds = request.TagIds.Distinct().ToHashSet();

        var removedTagIds = existingTagIds.Except(newTagIds).ToList();
        var addedTagIds = newTagIds.Except(existingTagIds).ToList();

        if (removedTagIds.Any())
        {
            var removedPostTags = existingPostTags.Where(x => removedTagIds.Contains(x.TagId)).ToList();
            _postTagRepository.RemoveRange(removedPostTags);

            var removedTags = await _tagRepository.GetByIdsAsync(removedTagIds, cancellationToken);
            foreach (var tag in removedTags)
            {
                if (tag.PostCount > 0)
                    tag.PostCount -= 1;

                _tagRepository.Update(tag);
            }
        }

        if (addedTagIds.Any())
        {
            var addedTags = await _tagRepository.GetByIdsAsync(addedTagIds, cancellationToken);

            if (addedTags.Count != addedTagIds.Count)
                throw new BadRequestException("One or more tags are invalid.");

            var postTagsToAdd = addedTags.Select(tag => new PostTag
            {
                Id = Guid.NewGuid(),
                PostId = post.Id,
                TagId = tag.Id,
                CreatedAt = _dateTimeService.UtcNow
            }).ToList();

            await _postTagRepository.AddRangeAsync(postTagsToAdd, cancellationToken);

            foreach (var tag in addedTags)
            {
                tag.PostCount += 1;
                _tagRepository.Update(tag);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var updatedPost = await _postRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Updated post not found.");

        return MapToPostDto(updatedPost);
    }

    public async Task DeletePostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var post = await _postRepository.GetByIdAsync(id, cancellationToken);
        if (post is null)
            throw new NotFoundException("Post not found.");

        if (post.AuthorId != currentUserId.Value)
            throw new ForbiddenAccessException("You are not allowed to delete this post.");

        if (post.PostType == "Question" && post.QuestionPost is not null)
        {
            if (!string.IsNullOrWhiteSpace(post.QuestionPost.ImgPublicId))
            {
                await _cloudinaryService.DeleteImageAsync(post.QuestionPost.ImgPublicId, cancellationToken);
            }

            post.QuestionPost.ImgUrl = null;
            post.QuestionPost.ImgPublicId = null;
            post.QuestionPost.UpdatedAt = _dateTimeService.UtcNow;
        }

        post.Status = "Deleted";
        post.DeletedAt = _dateTimeService.UtcNow;
        post.UpdatedAt = _dateTimeService.UtcNow;

        _postRepository.Update(post);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, CancellationToken cancellationToken)
    {
        var baseSlug = _slugService.GenerateSlug(title);
        var slug = baseSlug;
        var count = 1;

        while (await _postRepository.GetBySlugAsync(slug, cancellationToken) is not null)
        {
            slug = $"{baseSlug}-{count}";
            count++;
        }

        return slug;
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