namespace DevOpsAiHub.Application.Features.Auth.DTOs;

public class VerifyRegisterOtpRequestDto
{
    public string Email { get; set; } = null!;
    public string Otp { get; set; } = null!;
}