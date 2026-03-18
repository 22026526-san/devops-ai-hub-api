namespace DevOpsAiHub.Application.Common.Interfaces.Auth;

public interface IOtpService
{
    Task StoreRegisterOtpAsync(
        string email,
        string username,
        string passwordHash,
        string otp,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string Message)> VerifyRegisterOtpAsync(
        string email,
        string otp,
        CancellationToken cancellationToken = default);

    Task<(string Email, string Username, string PasswordHash)?> GetPendingRegisterAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task RemoveRegisterOtpAsync(string email, CancellationToken cancellationToken = default);

    Task StoreForgotPasswordOtpAsync(
        string email,
        string otp,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string Message)> VerifyForgotPasswordOtpAsync(
        string email,
        string otp,
        CancellationToken cancellationToken = default);

    Task RemoveForgotPasswordOtpAsync(string email, CancellationToken cancellationToken = default);
}