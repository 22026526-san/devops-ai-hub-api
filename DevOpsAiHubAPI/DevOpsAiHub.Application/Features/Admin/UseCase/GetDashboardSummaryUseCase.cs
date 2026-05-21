namespace DevOpsAiHub.Application.Features.Admin.UseCase;

using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.Admin.DTOs;



public class GetDashboardSummaryUseCase
{
    private readonly IDashboardService _dashboardService;

    public GetDashboardSummaryUseCase(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<DashboardSummaryResponseDto> ExecuteAsync(
        DashboardSummaryRequestDto request,
        CancellationToken ct = default)
    {
        var startDate = request.StartDate
            ?? new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var endDate = request.EndDate
            ?? new DateTime(DateTime.UtcNow.Year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        return await _dashboardService.GetSummaryAsync(startDate, endDate, ct);
    }
}
