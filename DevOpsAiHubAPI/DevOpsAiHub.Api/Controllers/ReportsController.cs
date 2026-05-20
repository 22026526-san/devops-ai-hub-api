using DevOpsAiHub.Application.Features.App.Reports.DTOs;
using DevOpsAiHub.Application.Features.App.Reports.Services;
using DevOpsAiHub.Application.Features.Users.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsAiHub.Api.Controllers;

[ApiController]
public class ReportsController : ControllerBase
{
    private readonly IReportAppService _reportAppService;

    public ReportsController(IReportAppService reportAppService)
    {
        _reportAppService = reportAppService;
    }

    [HttpPost("api/posts/{postId:guid}/report")]
    [Authorize]
    public async Task<IActionResult> Create(
        Guid postId,
        [FromBody] CreateReportRequestDto request,
        CancellationToken cancellationToken)
    {
        await _reportAppService.CreateAsync(postId, request, cancellationToken);
        return Ok(new { message = "Report submitted successfully." });
    }

    [HttpGet("api/reports")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] ReportQuery request,CancellationToken cancellationToken)
    {
        var result = await _reportAppService.GetAllAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("api/reports/{id:guid}/resolve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Resolve(
        Guid id,
        [FromBody] ReviewReportRequestDto request,
        CancellationToken cancellationToken)
    {
        await _reportAppService.ResolveAsync(id, request, cancellationToken);
        return Ok(new { message = "Report resolved successfully." });
    }

    [HttpPut("api/reports/{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] ReviewReportRequestDto request,
        CancellationToken cancellationToken)
    {
        await _reportAppService.RejectAsync(id, request, cancellationToken);
        return Ok(new { message = "Report rejected successfully." });
    }
}