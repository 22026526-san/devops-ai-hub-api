using DevOpsAiHub.Domain.Entities.Posts;

namespace DevOpsAiHub.Application.Common.Interfaces.Repositories;

public interface IReportRepository
{
    Task<List<Report>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsPendingReportAsync(Guid reporterId, Guid postId, CancellationToken cancellationToken = default);
    Task AddAsync(Report report, CancellationToken cancellationToken = default);
    void Update(Report report);
}