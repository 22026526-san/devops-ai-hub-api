namespace DevOpsAiHub.Application.Features.App.Tags.DTOs;
public class GetTagQueryDto
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

