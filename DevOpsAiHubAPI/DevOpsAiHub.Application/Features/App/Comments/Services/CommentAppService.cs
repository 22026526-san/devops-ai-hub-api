using DevOpsAiHub.Application.Common.Exceptions;
using DevOpsAiHub.Application.Common.Interfaces.Auth;
using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.App.Comments.DTOs;
using DevOpsAiHub.Application.Features.App.Comments.Services;
using DevOpsAiHub.Domain.Entities.Posts;

namespace DevOpsAiHub.Application.Features.Comments.Services;

public class CommentAppService : ICommentAppService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICloudinaryService _cloudinaryService;

    public CommentAppService(
        ICurrentUserService currentUserService,
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IApplicationDbContext context,
        IDateTimeService dateTimeService,
        ICloudinaryService cloudinaryService)
    {
        _currentUserService = currentUserService;
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _context = context;
        _dateTimeService = dateTimeService;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<List<CommentDto>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post is null)
            throw new NotFoundException("Post not found.");

        var comments = await _commentRepository.GetByPostIdAsync(postId, cancellationToken);

        var lookup = comments.ToDictionary(
            x => x.Id,
            x => new CommentDto
            {
                Id = x.Id,
                PostId = x.PostId,
                AuthorId = x.AuthorId,
                AuthorUsername = x.Author.Username,
                AuthorAvatarUrl = x.Author.Profile?.AvatarUrl,
                ParentCommentId = x.ParentCommentId,
                Content = x.Content,
                ImgUrl = x.ImgUrl,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Replies = new List<CommentDto>()
            });

        var roots = new List<CommentDto>();

        foreach (var comment in lookup.Values.OrderBy(x => x.CreatedAt))
        {
            if (comment.ParentCommentId.HasValue && lookup.TryGetValue(comment.ParentCommentId.Value, out var parent))
            {
                parent.Replies.Add(comment);
            }
            else
            {
                roots.Add(comment);
            }
        }

        return roots;
    }

    public async Task<CommentDto> CreateAsync(CreateCommentRequestDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        if (string.IsNullOrWhiteSpace(request.Content) && request.Image is null)
            throw new BadRequestException("Comment content or image is required.");

        var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            throw new NotFoundException("Post not found.");

        var now = _dateTimeService.UtcNow;
        string? imageUrl = null;
        string? imagePublicId = null;

        if (request.Image is not null && request.Image.Length > 0)
        {
            await using var stream = request.Image.OpenReadStream();
            var uploadResult = await _cloudinaryService.UploadImageAsync(stream, request.Image.FileName, cancellationToken);
            imageUrl = uploadResult.Url;
            imagePublicId = uploadResult.PublicId;
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            PostId = request.PostId,
            AuthorId = currentUserId.Value,
            ParentCommentId = null,
            Content = request.Content?.Trim() ?? string.Empty,
            ImgUrl = imageUrl,
            ImgPublicId = imagePublicId,
            Status = "Active",
            CreatedAt = now,
            UpdatedAt = now
        };

        await _commentRepository.AddAsync(comment, cancellationToken);

        post.CommentCount += 1;
        post.UpdatedAt = now;
        _postRepository.Update(post);

        await _context.SaveChangesAsync(cancellationToken);

        var created = await _commentRepository.GetByIdAsync(comment.Id, cancellationToken)
            ?? throw new NotFoundException("Created comment not found.");

        return MapToDto(created);
    }

    public async Task<CommentDto> ReplyAsync(Guid parentCommentId, ReplyCommentRequestDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        if (string.IsNullOrWhiteSpace(request.Content) && request.Image is null)
            throw new BadRequestException("Reply content or image is required.");

        var parentComment = await _commentRepository.GetByIdAsync(parentCommentId, cancellationToken);
        if (parentComment is null)
            throw new NotFoundException("Parent comment not found.");

        var post = await _postRepository.GetByIdAsync(parentComment.PostId, cancellationToken);
        if (post is null)
            throw new NotFoundException("Post not found.");

        var now = _dateTimeService.UtcNow;
        string? imageUrl = null;
        string? imagePublicId = null;

        if (request.Image is not null && request.Image.Length > 0)
        {
            await using var stream = request.Image.OpenReadStream();
            var uploadResult = await _cloudinaryService.UploadImageAsync(stream, request.Image.FileName, cancellationToken);
            imageUrl = uploadResult.Url;
            imagePublicId = uploadResult.PublicId;
        }

        var reply = new Comment
        {
            Id = Guid.NewGuid(),
            PostId = parentComment.PostId,
            AuthorId = currentUserId.Value,
            ParentCommentId = parentCommentId,
            Content = request.Content?.Trim() ?? string.Empty,
            ImgUrl = imageUrl,
            ImgPublicId = imagePublicId,
            Status = "Active",
            CreatedAt = now,
            UpdatedAt = now
        };

        await _commentRepository.AddAsync(reply, cancellationToken);

        post.CommentCount += 1;
        post.UpdatedAt = now;
        _postRepository.Update(post);

        await _context.SaveChangesAsync(cancellationToken);

        var created = await _commentRepository.GetByIdAsync(reply.Id, cancellationToken)
            ?? throw new NotFoundException("Created reply not found.");

        return MapToDto(created);
    }

    public async Task<CommentDto> UpdateAsync(Guid commentId, UpdateCommentRequestDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        if (string.IsNullOrWhiteSpace(request.Content) && request.Image is null)
            throw new BadRequestException("Comment content or image is required.");

        var comment = await _commentRepository.GetByIdAsync(commentId, cancellationToken);
        if (comment is null)
            throw new NotFoundException("Comment not found.");

        if (comment.AuthorId != currentUserId.Value)
            throw new ForbiddenAccessException("You are not allowed to update this comment.");

        string? imageUrl = comment.ImgUrl;
        string? imagePublicId = comment.ImgPublicId;

        if (request.Image is not null && request.Image.Length > 0)
        {
            if (!string.IsNullOrWhiteSpace(comment.ImgPublicId))
            {
                await _cloudinaryService.DeleteImageAsync(comment.ImgPublicId, cancellationToken);
            }

            await using var stream = request.Image.OpenReadStream();
            var uploadResult = await _cloudinaryService.UploadImageAsync(stream, request.Image.FileName, cancellationToken);
            imageUrl = uploadResult.Url;
            imagePublicId = uploadResult.PublicId;
        }

        comment.Content = request.Content?.Trim() ?? string.Empty;
        comment.ImgUrl = imageUrl;
        comment.ImgPublicId = imagePublicId;
        comment.UpdatedAt = _dateTimeService.UtcNow;

        _commentRepository.Update(comment);
        await _context.SaveChangesAsync(cancellationToken);

        var updated = await _commentRepository.GetByIdAsync(comment.Id, cancellationToken)
            ?? throw new NotFoundException("Updated comment not found.");

        return MapToDto(updated);
    }

    public async Task DeleteAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var comment = await _commentRepository.GetByIdAsync(commentId, cancellationToken);
        if (comment is null)
            throw new NotFoundException("Comment not found.");

        if (comment.AuthorId != currentUserId.Value)
            throw new ForbiddenAccessException("You are not allowed to delete this comment.");

        var post = await _postRepository.GetByIdAsync(comment.PostId, cancellationToken);
        if (post is null)
            throw new NotFoundException("Post not found.");

        if (!string.IsNullOrWhiteSpace(comment.ImgPublicId))
        {
            await _cloudinaryService.DeleteImageAsync(comment.ImgPublicId, cancellationToken);
        }

        comment.Status = "Deleted";
        comment.DeletedAt = _dateTimeService.UtcNow;
        comment.UpdatedAt = _dateTimeService.UtcNow;
        comment.Content = "[Deleted comment]";
        comment.ImgUrl = null;
        comment.ImgPublicId = null;

        _commentRepository.Update(comment);

        if (post.CommentCount > 0)
            post.CommentCount -= 1;

        post.UpdatedAt = _dateTimeService.UtcNow;
        _postRepository.Update(post);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static CommentDto MapToDto(Comment x)
    {
        return new CommentDto
        {
            Id = x.Id,
            PostId = x.PostId,
            AuthorId = x.AuthorId,
            AuthorUsername = x.Author.Username,
            AuthorAvatarUrl = x.Author.Profile?.AvatarUrl,
            ParentCommentId = x.ParentCommentId,
            Content = x.Content,
            ImgUrl = x.ImgUrl,
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            Replies = new List<CommentDto>()
        };
    }
}