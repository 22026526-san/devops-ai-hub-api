namespace DevOpsAiHub.Application.Features.Auth.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Role { get; set; } = null!;
}