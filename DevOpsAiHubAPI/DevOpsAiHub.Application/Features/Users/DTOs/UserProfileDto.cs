namespace DevOpsAiHub.Application.Features.Users.DTOs;

public class UserProfileDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? FullName { get; set; }
    public string? JobTitle { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? GithubUrl { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public string Role { get; set; } = null!;
    public string Status { get; set; } = null!;
    public bool? IsFollowing { get; set; }
    public DateTime? CreatedAt { get; set; }
}