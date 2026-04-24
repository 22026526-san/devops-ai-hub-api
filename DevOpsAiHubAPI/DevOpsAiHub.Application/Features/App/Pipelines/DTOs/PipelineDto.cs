namespace DevOpsAiHub.Application.Features.App.Pipelines.DTOs;

public class PipelineDto
{
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorUsername { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public int VersionCount { get; set; }
    public string? CurrentPipelineContent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}