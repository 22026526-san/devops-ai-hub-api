using DevOpsAiHub.Application.Features.App.Likes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsAiHub.Api.Controllers;

[ApiController]
public class LikesController : ControllerBase
{
    private readonly ILikeAppService _likeAppService;

    public LikesController(ILikeAppService likeAppService)
    {
        _likeAppService = likeAppService;
    }

    [HttpPost("api/posts/{postId:guid}/like")]
    [Authorize]
    public async Task<IActionResult> Like(Guid postId, CancellationToken cancellationToken)
    {
        await _likeAppService.LikeAsync(postId, cancellationToken);
        return Ok(new { message = "Liked successfully." });
    }

    [HttpDelete("api/posts/{postId:guid}/like")]
    [Authorize]
    public async Task<IActionResult> Unlike(Guid postId, CancellationToken cancellationToken)
    {
        await _likeAppService.UnlikeAsync(postId, cancellationToken);
        return Ok(new { message = "Unliked successfully." });
    }

    [HttpGet("api/users/me/likes")]
    [Authorize]
    public async Task<IActionResult> GetMyLikedPosts(CancellationToken cancellationToken)
    {
        var result = await _likeAppService.GetMyLikedPostsAsync(cancellationToken);
        return Ok(result);
    }
}