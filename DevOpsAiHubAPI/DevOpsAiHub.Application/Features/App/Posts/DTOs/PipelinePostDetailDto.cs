namespace DevOpsAiHub.Application.Features.App.Posts.DTOs;

public class PipelinePostDetailDto
{
    public string? Description { get; set; }
    public int VersionCount { get; set; }
    public string? PipelineContent { get; set; }
}