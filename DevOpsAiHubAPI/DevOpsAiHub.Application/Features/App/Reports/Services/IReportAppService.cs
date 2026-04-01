using DevOpsAiHub.Application.Features.App.Reports.DTOs;

namespace DevOpsAiHub.Application.Features.App.Reports.Services;

public interface IReportAppService
{
    Task CreateAsync(Guid postId, CreateReportRequestDto request, CancellationToken cancellationToken = default);
    Task<List<ReportDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task ResolveAsync(Guid reportId, ReviewReportRequestDto request, CancellationToken cancellationToken = default);
    Task RejectAsync(Guid reportId, ReviewReportRequestDto request, CancellationToken cancellationToken = default);
}