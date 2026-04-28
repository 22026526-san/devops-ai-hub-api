using DevOpsAiHub.Application.Common.Models;
using DevOpsAiHub.Application.Features.App.Likes.DTOs;
using DevOpsAiHub.Application.Features.App.Posts.DTOs;

namespace DevOpsAiHub.Application.Features.App.Likes.Services;

public interface ILikeAppService
{
    Task LikeAsync(Guid postId, CancellationToken cancellationToken = default);
    Task UnlikeAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<PagedResult<PostDto>> GetMyLikedPostsAsync(GetLikesQueryDto request,CancellationToken cancellationToken = default);
}