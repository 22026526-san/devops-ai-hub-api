namespace DevOpsAiHub.Application.Features.Users.DTOs;

public class UpdateProfileRequestDto
{
    public string? FullName { get; set; }
    public string? Bio { get; set; }
    public string? GithubUrl { get; set; }
}