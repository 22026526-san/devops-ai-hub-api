namespace DevOpsAiHub.Application.Features.Auth.DTOs;

public class LoginRequestDto
{
    public string EmailOrUsername { get; set; } = null!;
    public string Password { get; set; } = null!;
}