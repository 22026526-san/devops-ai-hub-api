using DevOpsAiHub.Application.Common.Exceptions;
using DevOpsAiHub.Application.Common.Interfaces.Auth;
using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.App.Pipelines.DTOs;
using DevOpsAiHub.Domain.Entities.Posts;

namespace DevOpsAiHub.Application.Features.Pipelines.Services;

public class PipelineAppService : IPipelineAppService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPipelineRepository _pipelineRepository;
    private readonly IPostRepository _postRepository;
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly ISlugService _slugService;

    public PipelineAppService(
        ICurrentUserService currentUserService,
        IPipelineRepository pipelineRepository,
        IPostRepository postRepository,
        IApplicationDbContext context,
        IDateTimeService dateTimeService,
        ISlugService slugService)
    {
        _currentUserService = currentUserService;
        _pipelineRepository = pipelineRepository;
        _postRepository = postRepository;
        _context = context;
        _dateTimeService = dateTimeService;
        _slugService = slugService;
    }

    public async Task<PipelineDto> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await _pipelineRepository.GetPipelinePostByPostIdAsync(postId, cancellationToken);
        if (post is null || post.PipelinePost is null)
            throw new NotFoundException("Pipeline not found.");

        return MapToDto(post);
    }

    public async Task<List<PipelineVersionDto>> GetVersionsAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await _pipelineRepository.GetPipelinePostByPostIdAsync(postId, cancellationToken);
        if (post is null || post.PipelinePost is null)
            throw new NotFoundException("Pipeline not found.");

        var versions = await _pipelineRepository.GetVersionsByPostIdAsync(postId, cancellationToken);

        return versions.Select(MapVersionToDto).ToList();
    }

    public async Task<PipelineVersionDto> GetVersionByIdAsync(Guid versionId, CancellationToken cancellationToken = default)
    {
        var version = await _pipelineRepository.GetVersionByIdAsync(versionId, cancellationToken);
        if (version is null)
            throw new NotFoundException("Pipeline version not found.");

        return MapVersionToDto(version);
    }

    public async Task<PipelineVersionDto> CreateVersionAsync(Guid postId, CreatePipelineVersionRequestDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new BadRequestException("Pipeline content is required.");

        var post = await _pipelineRepository.GetPipelinePostByPostIdAsync(postId, cancellationToken);
        if (post is null || post.PipelinePost is null)
            throw new NotFoundException("Pipeline not found.");

        if (post.AuthorId != currentUserId.Value)
            throw new ForbiddenAccessException("You are not allowed to create a version for this pipeline.");

        var nextVersionNumber = await _pipelineRepository.GetNextVersionNumberAsync(postId, cancellationToken);

        var version = new PipelineVersion
        {
            Id = Guid.NewGuid(),
            PipelinePostId = postId,
            VersionNumber = nextVersionNumber,
            Content = request.Content,
            Changelog = request.Changelog?.Trim(),
            CreatedBy = currentUserId.Value,
            CreatedAt = _dateTimeService.UtcNow
        };

        await _pipelineRepository.AddVersionAsync(version, cancellationToken);

        post.PipelinePost.CurrentVersionId = version.Id;
        post.PipelinePost.VersionCount = nextVersionNumber;
        post.PipelinePost.UpdatedAt = _dateTimeService.UtcNow;
        post.UpdatedAt = _dateTimeService.UtcNow;

        _postRepository.Update(post);
        await _context.SaveChangesAsync(cancellationToken);

        var createdVersion = await _pipelineRepository.GetVersionByIdAsync(version.Id, cancellationToken)
            ?? throw new NotFoundException("Created version not found.");

        return MapVersionToDto(createdVersion);
    }

    public async Task<PipelineDto> UpdateMetadataAsync(Guid postId, UpdatePipelineMetadataRequestDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var post = await _pipelineRepository.GetPipelinePostByPostIdAsync(postId, cancellationToken);
        if (post is null || post.PipelinePost is null)
            throw new NotFoundException("Pipeline not found.");

        if (post.AuthorId != currentUserId.Value)
            throw new ForbiddenAccessException("You are not allowed to update this pipeline.");

        if (string.IsNullOrWhiteSpace(request.Platform) ||
            string.IsNullOrWhiteSpace(request.PipelineFormat) ||
            string.IsNullOrWhiteSpace(request.ProjectType))
        {
            throw new BadRequestException("Invalid pipeline metadata.");
        }

        post.PipelinePost.Description = request.Description?.Trim();
        post.PipelinePost.Platform = request.Platform.Trim();
        post.PipelinePost.PipelineFormat = request.PipelineFormat.Trim();
        post.PipelinePost.ProjectType = request.ProjectType.Trim();
        post.PipelinePost.EnvironmentType = request.EnvironmentType?.Trim();
        post.PipelinePost.DeploymentTarget = request.DeploymentTarget?.Trim();
        post.PipelinePost.CiEnabled = request.CiEnabled;
        post.PipelinePost.CdEnabled = request.CdEnabled;
        post.PipelinePost.TestEnabled = request.TestEnabled;
        post.PipelinePost.SecurityScanEnabled = request.SecurityScanEnabled;
        post.PipelinePost.UpdatedAt = _dateTimeService.UtcNow;
        post.UpdatedAt = _dateTimeService.UtcNow;

        _postRepository.Update(post);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(post);
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

    private static PipelineDto MapToDto(Post post)
    {
        var pipeline = post.PipelinePost!;

        return new PipelineDto
        {
            PostId = post.Id,
            AuthorId = post.AuthorId,
            AuthorUsername = post.Author.Username,
            Title = post.Title,
            Slug = post.Slug,
            Summary = post.Summary,
            Description = pipeline.Description,
            Platform = pipeline.Platform,
            PipelineFormat = pipeline.PipelineFormat,
            ProjectType = pipeline.ProjectType,
            EnvironmentType = pipeline.EnvironmentType,
            DeploymentTarget = pipeline.DeploymentTarget,
            CiEnabled = pipeline.CiEnabled,
            CdEnabled = pipeline.CdEnabled,
            TestEnabled = pipeline.TestEnabled,
            SecurityScanEnabled = pipeline.SecurityScanEnabled,
            ForkCount = pipeline.ForkCount,
            VersionCount = pipeline.VersionCount,
            CurrentPipelineContent = pipeline.CurrentVersion?.Content,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }

    private static PipelineVersionDto MapVersionToDto(PipelineVersion version)
    {
        return new PipelineVersionDto
        {
            Id = version.Id,
            PipelinePostId = version.PipelinePostId,
            VersionNumber = version.VersionNumber,
            Content = version.Content,
            Changelog = version.Changelog,
            CreatedBy = version.CreatedBy,
            CreatedByUsername = version.Creator.Username,
            CreatedAt = version.CreatedAt
        };
    }
}