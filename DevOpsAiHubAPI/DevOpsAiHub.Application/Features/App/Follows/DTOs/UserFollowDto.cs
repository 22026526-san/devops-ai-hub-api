namespace DevOpsAiHub.Application.Features.App.Follows.DTOs;

public class UserFollowDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime FollowedAt { get; set; }
}