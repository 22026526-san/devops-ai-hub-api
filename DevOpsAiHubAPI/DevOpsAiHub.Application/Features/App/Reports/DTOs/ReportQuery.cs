namespace DevOpsAiHub.Application.Features.App.Reports.DTOs;

public class ReportQuery
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
