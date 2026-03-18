using DevOpsAiHub.Application.Features.Auth.DTOs;
using DevOpsAiHub.Application.Features.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsAiHub.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthAppService _authAppService;

    public AuthController(IAuthAppService authAppService)
    {
        _authAppService = authAppService;
    }

    [HttpPost("register/request-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestRegisterOtp(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        await _authAppService.RequestRegisterOtpAsync(request, cancellationToken);
        return Ok(new { message = "OTP has been sent to your email." });
    }

    [HttpPost("register/verify-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyRegisterOtp(
        [FromBody] VerifyRegisterOtpRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _authAppService.VerifyRegisterOtpAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _authAppService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("forgot-password/request-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestForgotPasswordOtp(
        [FromBody] ForgotPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        await _authAppService.RequestForgotPasswordOtpAsync(request, cancellationToken);
        return Ok(new { message = "OTP has been sent to your email." });
    }

    [HttpPost("forgot-password/reset")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        await _authAppService.ResetPasswordAsync(request, cancellationToken);
        return Ok(new { message = "Password reset successfully." });
    }
}