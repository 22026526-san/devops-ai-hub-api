using Microsoft.AspNetCore.Http;

namespace DevOpsAiHub.Application.Features.App.Posts.DTOs;

public class UpdatePostRequestDto
{
    public string Title { get; set; } = null!;
    public string? Summary { get; set; }
    public string Visibility { get; set; } = "Public";

    public string? QuestionContent { get; set; }
    public IFormFile? QuestionImage { get; set; }

    public string? PipelineDescription { get; set; }
    public string? Platform { get; set; }
    public string? PipelineFormat { get; set; }
    public string? ProjectType { get; set; }
    public string? EnvironmentType { get; set; }
    public string? DeploymentTarget { get; set; }
    public bool? CiEnabled { get; set; }
    public bool? CdEnabled { get; set; }
    public bool? TestEnabled { get; set; }
    public bool? SecurityScanEnabled { get; set; }
    public string? PipelineContent { get; set; }
    public string? Changelog { get; set; }
    public List<Guid> TagIds { get; set; } = new();
}