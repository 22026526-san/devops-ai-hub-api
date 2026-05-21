namespace DevOpsAiHub.Application.Common.Interfaces.Services;

using DevOpsAiHub.Application.Features.Admin.DTOs;

public interface IDashboardService
{
    Task<DashboardSummaryResponseDto> GetSummaryAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default);
}