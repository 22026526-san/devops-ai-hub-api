namespace DevOpsAiHub.Application.Features.App.Posts.DTOs;

public class PipelinePostDetailDto
{
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
    public int ForkCount { get; set; }
    public int VersionCount { get; set; }
    public string? PipelineContent { get; set; }
}