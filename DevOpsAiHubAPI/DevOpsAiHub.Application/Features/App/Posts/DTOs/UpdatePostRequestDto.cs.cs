using Microsoft.AspNetCore.Http;

namespace DevOpsAiHub.Application.Features.App.Posts.DTOs;

public class UpdatePostRequestDto
{
    public string Title { get; set; } = null!;
    public string Visibility { get; set; } = "Public";

    public string? QuestionContent { get; set; }
    public IFormFile? QuestionImage { get; set; }
    public string? PipelineDescription { get; set; }
    public string? PipelineContent { get; set; }
    public string? Changelog { get; set; }
    public List<Guid> TagIds { get; set; } = new();
}