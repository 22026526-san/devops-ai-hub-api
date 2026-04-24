namespace DevOpsAiHub.Application.Features.App.Posts.DTOs;

public class CreatePipelinePostRequestDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Content { get; set; } = null!;
    public string? Changelog { get; set; }
    public string Visibility { get; set; } = "Public";
    public List<Guid> TagIds { get; set; } = new();
}