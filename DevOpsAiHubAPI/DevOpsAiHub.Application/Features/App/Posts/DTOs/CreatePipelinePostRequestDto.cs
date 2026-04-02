namespace DevOpsAiHub.Application.Features.App.Posts.DTOs;

public class CreatePipelinePostRequestDto
{
    public string Title { get; set; } = null!;
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string Platform { get; set; } = null!;
    public string PipelineFormat { get; set; } = null!;
    public string ProjectType { get; set; } = null!;
    public string? EnvironmentType { get; set; }
    public string? DeploymentTarget { get; set; }
    public bool CiEnabled { get; set; }
    public bool CdEnabled { get; set; }
    public bool TestEnabled { get; set; }
    public bool SecurityScanEnabled { get; set; }
    public string Content { get; set; } = null!;
    public string? Changelog { get; set; }
    public string Visibility { get; set; } = "Public";
    public List<Guid> TagIds { get; set; } = new();
}