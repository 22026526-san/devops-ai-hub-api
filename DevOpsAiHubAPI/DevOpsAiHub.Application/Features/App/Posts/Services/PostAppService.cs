using DevOpsAiHub.Application.Common.Exceptions;
using DevOpsAiHub.Application.Common.Interfaces.Auth;
using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.App.Posts.DTOs;
using DevOpsAiHub.Application.Features.Posts.DTOs;
using DevOpsAiHub.Domain.Entities.Posts;

namespace DevOpsAiHub.Application.Features.Posts.Services;

public class PostAppService : IPostAppService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPostRepository _postRepository;
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly ISlugService _slugService;

    public PostAppService(
        ICurrentUserService currentUserService,
        IPostRepository postRepository,
        IApplicationDbContext context,
        IDateTimeService dateTimeService,
        ISlugService slugService)
    {
        _currentUserService = currentUserService;
        _postRepository = postRepository;
        _context = context;
        _dateTimeService = dateTimeService;
        _slugService = slugService;
    }

    public async Task<List<PostDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var posts = await _postRepository.GetAllAsync(cancellationToken);

        return posts.Select(x => new PostDto
        {
            Id = x.Id,
            AuthorId = x.AuthorId,
            AuthorUsername = x.Author.Username,
            PostType = x.PostType,
            Title = x.Title,
            Slug = x.Slug,
            Summary = x.Summary,
            Status = x.Status,
            Visibility = x.Visibility,
            ViewCount = x.ViewCount,
            LikeCount = x.LikeCount,
            CommentCount = x.CommentCount,
            BookmarkCount = x.BookmarkCount,
            CreatedAt = x.CreatedAt
        }).ToList();
    }

    public async Task<PostDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(id, cancellationToken);
        if (post is null)
            throw new NotFoundException("Post not found.");

        post.ViewCount += 1;
        post.UpdatedAt = _dateTimeService.UtcNow;
        _postRepository.Update(post);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDetailDto(post);
    }

    public async Task<PostDetailDto> CreateQuestionPostAsync(CreateQuestionPostRequestDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new BadRequestException("Title is required.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new BadRequestException("Question content is required.");

        var now = _dateTimeService.UtcNow;
        var postId = Guid.NewGuid();

        var post = new Post
        {
            Id = postId,
            AuthorId = currentUserId.Value,
            PostType = "Question",
            Title = request.Title.Trim(),
            Slug = await GenerateUniqueSlugAsync(request.Title.Trim(), cancellationToken),
            Summary = request.Summary?.Trim(),
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
            Content = request.Content.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _postRepository.AddAsync(post, cancellationToken);
        await _context.QuestionPosts.AddAsync(questionPost, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var createdPost = await _postRepository.GetByIdAsync(postId, cancellationToken)
            ?? throw new NotFoundException("Created post not found.");

        return MapToDetailDto(createdPost);
    }

    public async Task<PostDetailDto> CreatePipelinePostAsync(CreatePipelinePostRequestDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new BadRequestException("Title is required.");

        if (string.IsNullOrWhiteSpace(request.Platform))
            throw new BadRequestException("Platform is required.");

        if (string.IsNullOrWhiteSpace(request.PipelineFormat))
            throw new BadRequestException("Pipeline format is required.");

        if (string.IsNullOrWhiteSpace(request.ProjectType))
            throw new BadRequestException("Project type is required.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new BadRequestException("Pipeline content is required.");

        var now = _dateTimeService.UtcNow;
        var postId = Guid.NewGuid();
        var versionId = Guid.NewGuid();

        var post = new Post
        {
            Id = postId,
            AuthorId = currentUserId.Value,
            PostType = "Pipeline",
            Title = request.Title.Trim(),
            Slug = await GenerateUniqueSlugAsync(request.Title.Trim(), cancellationToken),
            Summary = request.Summary?.Trim(),
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
            CurrentVersionId = versionId,
            Description = request.Description?.Trim(),
            Platform = request.Platform.Trim(),
            PipelineFormat = request.PipelineFormat.Trim(),
            ProjectType = request.ProjectType.Trim(),
            EnvironmentType = request.EnvironmentType?.Trim(),
            DeploymentTarget = request.DeploymentTarget?.Trim(),
            CiEnabled = request.CiEnabled,
            CdEnabled = request.CdEnabled,
            TestEnabled = request.TestEnabled,
            SecurityScanEnabled = request.SecurityScanEnabled,
            ForkCount = 0,
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

        await _postRepository.AddAsync(post, cancellationToken);
        await _context.PipelinePosts.AddAsync(pipelinePost, cancellationToken);
        await _context.PipelineVersions.AddAsync(version, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var createdPost = await _postRepository.GetByIdAsync(postId, cancellationToken)
            ?? throw new NotFoundException("Created pipeline post not found.");

        return MapToDetailDto(createdPost);
    }

    public async Task<PostDetailDto> UpdatePostAsync(Guid id, UpdatePostRequestDto request, CancellationToken cancellationToken = default)
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
        post.Summary = request.Summary?.Trim();
        post.Visibility = request.Visibility;
        post.UpdatedAt = _dateTimeService.UtcNow;

        if (post.PostType == "Question")
        {
            if (post.QuestionPost is null)
                throw new BadRequestException("Question content not found.");

            if (string.IsNullOrWhiteSpace(request.QuestionContent))
                throw new BadRequestException("Question content is required.");

            post.QuestionPost.Content = request.QuestionContent.Trim();
            post.QuestionPost.UpdatedAt = _dateTimeService.UtcNow;
        }
        else if (post.PostType == "Pipeline")
        {
            if (post.PipelinePost is null)
                throw new BadRequestException("Pipeline post not found.");

            if (string.IsNullOrWhiteSpace(request.Platform) ||
                string.IsNullOrWhiteSpace(request.PipelineFormat) ||
                string.IsNullOrWhiteSpace(request.ProjectType) ||
                string.IsNullOrWhiteSpace(request.PipelineContent))
            {
                throw new BadRequestException("Pipeline data is invalid.");
            }

            post.PipelinePost.Description = request.PipelineDescription?.Trim();
            post.PipelinePost.Platform = request.Platform.Trim();
            post.PipelinePost.PipelineFormat = request.PipelineFormat.Trim();
            post.PipelinePost.ProjectType = request.ProjectType.Trim();
            post.PipelinePost.EnvironmentType = request.EnvironmentType?.Trim();
            post.PipelinePost.DeploymentTarget = request.DeploymentTarget?.Trim();
            post.PipelinePost.CiEnabled = request.CiEnabled ?? post.PipelinePost.CiEnabled;
            post.PipelinePost.CdEnabled = request.CdEnabled ?? post.PipelinePost.CdEnabled;
            post.PipelinePost.TestEnabled = request.TestEnabled ?? post.PipelinePost.TestEnabled;
            post.PipelinePost.SecurityScanEnabled = request.SecurityScanEnabled ?? post.PipelinePost.SecurityScanEnabled;
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
        await _context.SaveChangesAsync(cancellationToken);

        var updatedPost = await _postRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Updated post not found.");

        return MapToDetailDto(updatedPost);
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

    private static PostDetailDto MapToDetailDto(Post post)
    {
        return new PostDetailDto
        {
            Id = post.Id,
            AuthorId = post.AuthorId,
            AuthorUsername = post.Author.Username,
            PostType = post.PostType,
            Title = post.Title,
            Slug = post.Slug,
            Summary = post.Summary,
            Status = post.Status,
            Visibility = post.Visibility,
            ViewCount = post.ViewCount,
            LikeCount = post.LikeCount,
            CommentCount = post.CommentCount,
            BookmarkCount = post.BookmarkCount,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            QuestionContent = post.QuestionPost?.Content,
            PipelineDescription = post.PipelinePost?.Description,
            Platform = post.PipelinePost?.Platform,
            PipelineFormat = post.PipelinePost?.PipelineFormat,
            ProjectType = post.PipelinePost?.ProjectType,
            EnvironmentType = post.PipelinePost?.EnvironmentType,
            DeploymentTarget = post.PipelinePost?.DeploymentTarget,
            CiEnabled = post.PipelinePost?.CiEnabled,
            CdEnabled = post.PipelinePost?.CdEnabled,
            TestEnabled = post.PipelinePost?.TestEnabled,
            SecurityScanEnabled = post.PipelinePost?.SecurityScanEnabled,
            ForkCount = post.PipelinePost?.ForkCount,
            VersionCount = post.PipelinePost?.VersionCount,
            CurrentPipelineContent = post.PipelinePost?.CurrentVersion?.Content
        };
    }
}