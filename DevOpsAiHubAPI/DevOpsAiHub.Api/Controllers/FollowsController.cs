using DevOpsAiHub.Application.Features.App.Follows.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsAiHub.Api.Controllers;

[ApiController]
[Route("api/users")]
public class FollowsController : ControllerBase
{
    private readonly IFollowAppService _followAppService;

    public FollowsController(IFollowAppService followAppService)
    {
        _followAppService = followAppService;
    }

    [HttpPost("{userId:guid}/follow")]
    [Authorize]
    public async Task<IActionResult> Follow(Guid userId, CancellationToken cancellationToken)
    {
        await _followAppService.FollowUserAsync(userId, cancellationToken);
        return Ok(new { message = "Followed successfully." });
    }

    [HttpDelete("{userId:guid}/follow")]
    [Authorize]
    public async Task<IActionResult> Unfollow(Guid userId, CancellationToken cancellationToken)
    {
        await _followAppService.UnfollowUserAsync(userId, cancellationToken);
        return Ok(new { message = "Unfollowed successfully." });
    }

    [HttpGet("{userId:guid}/followers")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFollowers(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _followAppService.GetFollowersAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{userId:guid}/following")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFollowing(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _followAppService.GetFollowingAsync(userId, cancellationToken);
        return Ok(result);
    }
}