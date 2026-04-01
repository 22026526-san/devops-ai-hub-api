using DevOpsAiHub.Application.Features.App.Comments.DTOs;

namespace DevOpsAiHub.Application.Features.App.Comments.Services;

public interface ICommentAppService
{
    Task<List<CommentDto>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<CommentDto> CreateAsync(CreateCommentRequestDto request, CancellationToken cancellationToken = default);
    Task<CommentDto> ReplyAsync(Guid parentCommentId, ReplyCommentRequestDto request, CancellationToken cancellationToken = default);
    Task<CommentDto> UpdateAsync(Guid commentId, UpdateCommentRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid commentId, CancellationToken cancellationToken = default);
}