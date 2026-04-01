using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Domain.Entities.Posts;
using Microsoft.EntityFrameworkCore;

namespace DevOpsAiHub.Infrastructure.Persistence.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly IApplicationDbContext _context;

    public ReportRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Report>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .Include(x => x.Reporter)
            .Include(x => x.Post)
            .Include(x => x.Reviewer)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .Include(x => x.Reporter)
            .Include(x => x.Post)
            .Include(x => x.Reviewer)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsPendingReportAsync(Guid reporterId, Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .AnyAsync(x =>
                x.ReporterId == reporterId &&
                x.PostId == postId &&
                x.Status == "Pending",
                cancellationToken);
    }

    public async Task AddAsync(Report report, CancellationToken cancellationToken = default)
    {
        await _context.Reports.AddAsync(report, cancellationToken);
    }

    public void Update(Report report)
    {
        _context.Reports.Update(report);
    }
}