namespace DevOpsAiHub.Application.Features.App.Reports.DTOs;

public class CreateReportRequestDto
{
    public string Reason { get; set; } = null!;
    public string? Description { get; set; }
}