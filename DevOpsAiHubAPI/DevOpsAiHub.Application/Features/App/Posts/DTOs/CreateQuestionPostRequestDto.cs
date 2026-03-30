namespace DevOpsAiHub.Application.Features.Posts.DTOs;

public class CreateQuestionPostRequestDto
{
    public string Title { get; set; } = null!;
    public string? Summary { get; set; }
    public string Content { get; set; } = null!;
    public string Visibility { get; set; } = "Public";
}