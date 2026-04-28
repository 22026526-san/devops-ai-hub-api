using DevOpsAiHub.Application.Features.App.Bookmarks.DTOs;
using DevOpsAiHub.Application.Features.App.Bookmarks.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsAiHub.Api.Controllers;

[ApiController]
public class BookmarksController : ControllerBase
{
    private readonly IBookmarkAppService _bookmarkAppService;

    public BookmarksController(IBookmarkAppService bookmarkAppService)
    {
        _bookmarkAppService = bookmarkAppService;
    }

    [HttpPost("api/posts/{postId:guid}/bookmark")]
    [Authorize]
    public async Task<IActionResult> Bookmark(Guid postId, CancellationToken cancellationToken)
    {
        await _bookmarkAppService.BookmarkAsync(postId, cancellationToken);
        return Ok(new { message = "Bookmarked successfully." });
    }

    [HttpDelete("api/posts/{postId:guid}/bookmark")]
    [Authorize]
    public async Task<IActionResult> Unbookmark(Guid postId, CancellationToken cancellationToken)
    {
        await _bookmarkAppService.UnbookmarkAsync(postId, cancellationToken);
        return Ok(new { message = "Bookmark removed successfully." });
    }

    [HttpGet("api/users/me/bookmarks")]
    [Authorize]
    public async Task<IActionResult> GetMyBookmarks([FromQuery] GetBookmarksQueryDto request, CancellationToken cancellationToken)
    {
        var result = await _bookmarkAppService.GetMyBookmarksAsync( request,cancellationToken);
        return Ok(result);
    }
}