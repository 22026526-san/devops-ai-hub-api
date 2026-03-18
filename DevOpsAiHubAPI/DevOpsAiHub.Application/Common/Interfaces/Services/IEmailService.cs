namespace DevOpsAiHub.Application.Common.Interfaces.Services;

public interface IEmailService
{
    Task SendOtpAsync(string email, string otp, string subject, CancellationToken cancellationToken = default);
}