using DevOpsAiHub.Application.Features.App.Posts.DTOs;
using DevOpsAiHub.Application.Features.App.Bookmarks.DTOs;
using DevOpsAiHub.Application.Common.Models;

namespace DevOpsAiHub.Application.Features.App.Bookmarks.Services;

public interface IBookmarkAppService
{
    Task BookmarkAsync(Guid postId, CancellationToken cancellationToken = default);
    Task UnbookmarkAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<PagedResult<PostDto>> GetMyBookmarksAsync(GetBookmarksQueryDto request, CancellationToken cancellationToken = default);
}