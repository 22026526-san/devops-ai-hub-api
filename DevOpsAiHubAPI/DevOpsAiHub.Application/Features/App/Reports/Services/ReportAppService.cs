using DevOpsAiHub.Application.Common.Exceptions;
using DevOpsAiHub.Application.Common.Interfaces.Auth;
using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.App.Reports.DTOs;
using DevOpsAiHub.Domain.Entities.Posts;

namespace DevOpsAiHub.Application.Features.App.Reports.Services;

public class ReportAppService : IReportAppService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IReportRepository _reportRepository;
    private readonly IPostRepository _postRepository;
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;

    public ReportAppService(
        ICurrentUserService currentUserService,
        IReportRepository reportRepository,
        IPostRepository postRepository,
        IApplicationDbContext context,
        IDateTimeService dateTimeService)
    {
        _currentUserService = currentUserService;
        _reportRepository = reportRepository;
        _postRepository = postRepository;
        _context = context;
        _dateTimeService = dateTimeService;
    }

    public async Task CreateAsync(Guid postId, CreateReportRequestDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new BadRequestException("Reason is required.");

        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post is null)
            throw new NotFoundException("Post not found.");

        if (post.DeletedAt is not null || post.Status == "Deleted")
            throw new BadRequestException("Post is not available.");

        var alreadyReported = await _reportRepository.ExistsPendingReportAsync(currentUserId.Value, postId, cancellationToken);
        if (alreadyReported)
            throw new BadRequestException("You already reported this post.");

        var report = new Report
        {
            Id = Guid.NewGuid(),
            ReporterId = currentUserId.Value,
            PostId = postId,
            Reason = request.Reason.Trim(),
            Description = request.Description?.Trim(),
            Status = "Pending",
            ReviewedBy = null,
            ReviewNote = null,
            CreatedAt = _dateTimeService.UtcNow,
            ReviewedAt = null
        };

        await _reportRepository.AddAsync(report, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<ReportDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var reports = await _reportRepository.GetAllAsync(cancellationToken);

        return reports.Select(x => new ReportDto
        {
            Id = x.Id,
            ReporterId = x.ReporterId,
            ReporterUsername = x.Reporter.Username,
            PostId = x.PostId,
            PostTitle = x.Post.Title,
            Reason = x.Reason,
            Description = x.Description,
            Status = x.Status,
            ReviewedBy = x.ReviewedBy,
            ReviewerUsername = x.Reviewer?.Username,
            ReviewNote = x.ReviewNote,
            CreatedAt = x.CreatedAt,
            ReviewedAt = x.ReviewedAt
        }).ToList();
    }

    public async Task ResolveAsync(Guid reportId, ReviewReportRequestDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var report = await _reportRepository.GetByIdAsync(reportId, cancellationToken);
        if (report is null)
            throw new NotFoundException("Report not found.");

        report.Status = "Resolved";
        report.ReviewedBy = currentUserId.Value;
        report.ReviewNote = request.ReviewNote?.Trim();
        report.ReviewedAt = _dateTimeService.UtcNow;

        _reportRepository.Update(report);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectAsync(Guid reportId, ReviewReportRequestDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var report = await _reportRepository.GetByIdAsync(reportId, cancellationToken);
        if (report is null)
            throw new NotFoundException("Report not found.");

        report.Status = "Rejected";
        report.ReviewedBy = currentUserId.Value;
        report.ReviewNote = request.ReviewNote?.Trim();
        report.ReviewedAt = _dateTimeService.UtcNow;

        _reportRepository.Update(report);
        await _context.SaveChangesAsync(cancellationToken);
    }
}