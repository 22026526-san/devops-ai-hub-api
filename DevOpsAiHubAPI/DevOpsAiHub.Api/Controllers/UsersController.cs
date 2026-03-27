using DevOpsAiHub.Application.Features.Users.DTOs;
using DevOpsAiHub.Application.Features.Users.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsAiHub.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserAppService _userAppService;

    public UsersController(IUserAppService userAppService)
    {
        _userAppService = userAppService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await _userAppService.GetMyProfileAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{userId:guid}/profile")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserProfile(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _userAppService.GetUserProfileByIdAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("profiles")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllProfiles(CancellationToken cancellationToken)
    {
        var result = await _userAppService.GetAllProfilesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        await _userAppService.UpdateProfileAsync(request, cancellationToken);
        return Ok(new { message = "Profile updated successfully." });
    }

    [HttpPut("avatar")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateAvatar(
    [FromForm] UploadAvatarRequestDto request,
    CancellationToken cancellationToken)
    {
        var result = await _userAppService.UpdateAvatarAsync(request.File, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("avatar")]
    public async Task<IActionResult> RemoveAvatar(CancellationToken cancellationToken)
    {
        await _userAppService.RemoveAvatarAsync(cancellationToken);
        return Ok(new { message = "Avatar removed successfully." });
    }
}