namespace DevOpsAiHub.Application.Features.Users.DTOs;

public class VerifyUpdateUserOtpDto
{
    public string Otp { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
}