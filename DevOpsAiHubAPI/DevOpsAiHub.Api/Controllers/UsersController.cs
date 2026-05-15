using DevOpsAiHub.Application.Features.App.Posts.DTOs;
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
    public async Task<IActionResult> GetAllProfiles([FromQuery] GetUserQueryDto request,CancellationToken cancellationToken)
    {
        var result = await _userAppService.GetAllProfilesAsync(request,cancellationToken);
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

    [HttpGet("suggested-follows")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSuggestedProfiles(CancellationToken cancellationToken)
    {
        var result = await _userAppService.GetSuggestedProfilesAsync(cancellationToken);
        return Ok(result);
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
    [HttpPut("{userId:guid}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUserRole(
    Guid userId,
    [FromBody] UpdateUserRoleRequestDto request,
    CancellationToken cancellationToken)
    {
        await _userAppService.UpdateUserRoleAsync(userId, request, cancellationToken);
        return Ok(new { message = "User role updated successfully." });
    }

    [HttpPut("{userId:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUserStatus(
        Guid userId,
        [FromBody] UpdateUserStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        await _userAppService.UpdateUserStatusAsync(userId, request, cancellationToken);
        return Ok(new { message = "User status updated successfully." });
    }
    [HttpPost("request-update-user-otp")]
    [Authorize]
    public async Task<IActionResult> RequestUpdateUserOtp(
    [FromBody] UpdateUserOtpRequestDto request,
    CancellationToken cancellationToken)
    {
        await _userAppService.UpdateUserOtpRequestAsync(request, cancellationToken);
        return Ok(new { message = "OTP has been sent to your current email." });
    }

    [HttpPost("verify-update-user-otp")]
    [Authorize]
    public async Task<IActionResult> VerifyUpdateUserOtp(
        [FromBody] VerifyUpdateUserOtpDto request,
        CancellationToken cancellationToken)
    {
        await _userAppService.VerifyUpdateUserOtpAsync(request, cancellationToken);
        return Ok(new { message = "User updated successfully." });
    }
}