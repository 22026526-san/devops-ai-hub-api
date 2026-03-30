namespace DevOpsAiHub.Application.Features.App.Pipelines.DTOs;

public class CreatePipelineVersionRequestDto
{
    public string Content { get; set; } = null!;
    public string? Changelog { get; set; }
}