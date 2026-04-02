using Microsoft.AspNetCore.Http;

namespace DevOpsAiHub.Application.Features.App.Posts.DTOs;

public class CreateQuestionPostRequestDto
{
    public string Title { get; set; } = null!;
    public string? Summary { get; set; }
    public string Content { get; set; } = null!;
    public string Visibility { get; set; } = "Public";
    public IFormFile? Image { get; set; }
    public List<Guid> TagIds { get; set; } = new();
}