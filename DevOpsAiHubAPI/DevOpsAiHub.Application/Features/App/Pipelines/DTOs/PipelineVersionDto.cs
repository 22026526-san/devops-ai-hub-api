namespace DevOpsAiHub.Application.Features.App.Pipelines.DTOs;

public class PipelineVersionDto
{
    public Guid Id { get; set; }
    public Guid PipelinePostId { get; set; }
    public int VersionNumber { get; set; }
    public string Content { get; set; } = null!;
    public string? Changelog { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatedByUsername { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}