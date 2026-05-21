namespace DevOpsAiHub.Application.Features.Admin.DTOs;

public record DashboardSummaryRequestDto(
    DateTime? StartDate = null,
    DateTime? EndDate = null
);

public record DashboardSummaryResponseDto(
    bool Success,
    DashboardDataDto Data
);

public record DashboardDataDto(
    DashboardKpisDto Kpis,
    DashboardChartDto Chart
);

public record DashboardKpisDto(
    KpiItemDto TotalArticles,
    KpiItemDto TotalUsers,
    KpiItemDto UnresolvedViolations
);

public record KpiItemDto(
    int Value,
    double TrendPercentage,
    bool IsTrendUp
);

public record DashboardChartDto(
    string[] SeriesName,
    IReadOnlyList<ChartDataPointDto> DataPoints
);

public record ChartDataPointDto(
    string Date,
    int NewUsers,
    int NewArticles
);