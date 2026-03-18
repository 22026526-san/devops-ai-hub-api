namespace DevOpsAiHub.Application.Features.Users.DTOs;

public class UserProfileDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? GithubUrl { get; set; }
}