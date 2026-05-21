namespace DevOpsAiHub.Api.Controllers;
using DevOpsAiHub.Application.Features.Admin.DTOs;
using DevOpsAiHub.Application.Features.Admin.UseCase;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly GetDashboardSummaryUseCase _useCase;

    public DashboardController(
        GetDashboardSummaryUseCase useCase)
    {
        _useCase = useCase;
    }
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken ct)
    {
        if (startDate.HasValue != endDate.HasValue)
            return BadRequest(new
            {
                success = false,
                error = "Both startDate and endDate must be provided together, or both omitted."
            });

        if (startDate.HasValue && startDate.Value > endDate!.Value)
            return BadRequest(new
            {
                success = false,
                error = "startDate must be before or equal to endDate."
            });

        var request = new DashboardSummaryRequestDto(
            StartDate: startDate.HasValue
                ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc)
                : null,
            EndDate: endDate.HasValue
                ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)
                : null
        );

        var result = await _useCase.ExecuteAsync(request, ct);
        return Ok(result);
    }
}
