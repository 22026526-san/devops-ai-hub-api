namespace DevOpsAiHub.Infrastructure.Services;

using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.Admin.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _db;

    public DashboardService(IApplicationDbContext db) => _db = db;

    public async Task<DashboardSummaryResponseDto> GetSummaryAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default)
    {
        var kpis = await GetKpisAsync(startDate, endDate, ct);
        var chart = await GetChartDataAsync(startDate, endDate, ct);

        return new DashboardSummaryResponseDto(
            Success: true,
            Data: new DashboardDataDto(
                Kpis: kpis,
                Chart: chart
            )
        );
    }

    private async Task<DashboardKpisDto> GetKpisAsync(
        DateTime startDate, DateTime endDate, CancellationToken ct)
    {
        var periodLength = endDate - startDate;
        var prevStartDate = startDate - periodLength;
        var prevEndDate = startDate;

        var totalArticles = await _db.Posts
            .CountAsync(p => p.DeletedAt == null, ct);

        var currentArticles = await _db.Posts
            .CountAsync(p => p.CreatedAt >= startDate
                          && p.CreatedAt <= endDate
                          && p.DeletedAt == null, ct);

        var prevArticles = await _db.Posts
            .CountAsync(p => p.CreatedAt >= prevStartDate
                          && p.CreatedAt < prevEndDate
                          && p.DeletedAt == null, ct);

        var totalUsers = await _db.Users
            .CountAsync(ct);

        var currentUsers = await _db.Users
            .CountAsync(u => u.CreatedAt >= startDate
                          && u.CreatedAt <= endDate, ct);

        var prevUsers = await _db.Users
            .CountAsync(u => u.CreatedAt >= prevStartDate
                          && u.CreatedAt < prevEndDate, ct);

        var currentViolations = await _db.Reports
            .CountAsync(r => r.Status == "pending"
                          && r.CreatedAt >= startDate
                          && r.CreatedAt <= endDate, ct);

        var prevViolations = await _db.Reports
            .CountAsync(r => r.Status == "pending"
                          && r.CreatedAt >= prevStartDate
                          && r.CreatedAt < prevEndDate, ct);

        var totalViolations = await _db.Reports
            .CountAsync(r => r.Status == "pending", ct);

        // Đã xóa bỏ Task.WhenAll ở đây vì dữ liệu đã được await ở trên

        return new DashboardKpisDto(
            TotalArticles: BuildKpi(totalArticles, currentArticles, prevArticles),
            TotalUsers: BuildKpi(totalUsers, currentUsers, prevUsers),
            UnresolvedViolations: BuildKpi(totalViolations, currentViolations, prevViolations)
        );
    }

    private async Task<DashboardChartDto> GetChartDataAsync(
        DateTime startDate, DateTime endDate, CancellationToken ct)
    {
        var granularity = GetGranularity(startDate, endDate);

        var userPoints = await GetUserDataPointsAsync(startDate, endDate, granularity, ct);
        var articlePoints = await GetArticleDataPointsAsync(startDate, endDate, granularity, ct);

        var allDates = userPoints.Keys.Union(articlePoints.Keys).OrderBy(d => d);

        var dataPoints = allDates.Select(date => new ChartDataPointDto(
            Date: date.ToString("yyyy-MM-dd"),
            NewUsers: userPoints.GetValueOrDefault(date, 0),
            NewArticles: articlePoints.GetValueOrDefault(date, 0)
        )).ToList();

        return new DashboardChartDto(
            SeriesName: new[] { "Người dùng mới", "Bài viết mới" },
            DataPoints: dataPoints
        );
    }

    private async Task<Dictionary<DateTime, int>> GetUserDataPointsAsync(
        DateTime startDate, DateTime endDate,
        ChartGranularity granularity, CancellationToken ct)
    {
        var users = await _db.Users
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
            .Select(u => u.CreatedAt)
            .ToListAsync(ct);

        return GroupByGranularity(users, granularity);
    }

    private async Task<Dictionary<DateTime, int>> GetArticleDataPointsAsync(
        DateTime startDate, DateTime endDate,
        ChartGranularity granularity, CancellationToken ct)
    {
        var articles = await _db.Posts
            .Where(p => p.CreatedAt >= startDate
                     && p.CreatedAt <= endDate
                     && p.DeletedAt == null)
            .Select(p => p.CreatedAt)
            .ToListAsync(ct);

        return GroupByGranularity(articles, granularity);
    }

    private static KpiItemDto BuildKpi(int total, int current, int previous)
    {
        double trend = previous == 0
            ? (current > 0 ? 100.0 : 0.0)
            : Math.Round((double)(current - previous) / previous * 100, 1);

        return new KpiItemDto(
            Value: total,
            TrendPercentage: Math.Abs(trend),
            IsTrendUp: trend >= 0
        );
    }

    private static ChartGranularity GetGranularity(DateTime start, DateTime end)
    {
        var days = (end - start).TotalDays;
        return days switch
        {
            <= 14 => ChartGranularity.Daily,
            <= 90 => ChartGranularity.Weekly,
            <= 366 => ChartGranularity.Monthly,
            _ => ChartGranularity.Monthly
        };
    }

    private static Dictionary<DateTime, int> GroupByGranularity(
        IEnumerable<DateTime> dates, ChartGranularity granularity)
    {
        return granularity switch
        {
            ChartGranularity.Daily => dates
                .GroupBy(d => d.Date)
                .ToDictionary(g => g.Key, g => g.Count()),

            ChartGranularity.Weekly => dates
                .GroupBy(d => d.Date.AddDays(-(int)d.DayOfWeek))
                .ToDictionary(g => g.Key, g => g.Count()),

            ChartGranularity.Monthly => dates
                .GroupBy(d => new DateTime(d.Year, d.Month, 1))
                .ToDictionary(g => g.Key, g => g.Count()),

            _ => dates
                .GroupBy(d => new DateTime(d.Year, d.Month, 1))
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    private enum ChartGranularity { Daily, Weekly, Monthly }
}