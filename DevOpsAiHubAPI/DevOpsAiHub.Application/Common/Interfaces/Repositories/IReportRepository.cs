using DevOpsAiHub.Domain.Entities.Posts;

namespace DevOpsAiHub.Application.Common.Interfaces.Repositories;

public interface IReportRepository
{
    IQueryable<Report> Query();
    Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsPendingReportAsync(Guid reporterId, Guid postId, CancellationToken cancellationToken = default);
    Task AddAsync(Report report, CancellationToken cancellationToken = default);
    void Update(Report report);
}