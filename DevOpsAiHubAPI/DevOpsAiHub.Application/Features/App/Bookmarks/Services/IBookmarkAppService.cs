using DevOpsAiHub.Application.Features.App.Posts.DTOs;

namespace DevOpsAiHub.Application.Features.App.Bookmarks.Services;

public interface IBookmarkAppService
{
    Task BookmarkAsync(Guid postId, CancellationToken cancellationToken = default);
    Task UnbookmarkAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<List<PostDto>> GetMyBookmarksAsync(CancellationToken cancellationToken = default);
}