namespace DevOpsAiHub.Application.Features.App.Reports.DTOs;

public class ReportDto
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public string ReporterUsername { get; set; } = null!;
    public Guid PostId { get; set; }
    public string PostTitle { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public string? Description { get; set; }
    public string Status { get; set; } = null!;
    public Guid? ReviewedBy { get; set; }
    public string? ReviewerUsername { get; set; }
    public string? ReviewNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}