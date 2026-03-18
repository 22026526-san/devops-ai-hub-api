using DevOpsAiHub.Application.Features.Auth.DTOs;

namespace DevOpsAiHub.Application.Features.Auth.Services;

public interface IAuthAppService
{
    Task RequestRegisterOtpAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> VerifyRegisterOtpAsync(VerifyRegisterOtpRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task RequestForgotPasswordOtpAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default);
}