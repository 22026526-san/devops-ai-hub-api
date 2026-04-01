using DevOpsAiHub.Application.Features.App.Pipelines.DTOs;
using DevOpsAiHub.Application.Features.App.Pipelines.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsAiHub.Api.Controllers;

[ApiController]
[Route("api/pipelines")]
public class PipelinesController : ControllerBase
{
    private readonly IPipelineAppService _pipelineAppService;

    public PipelinesController(IPipelineAppService pipelineAppService)
    {
        _pipelineAppService = pipelineAppService;
    }

    [HttpGet("{postId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByPostId(Guid postId, CancellationToken cancellationToken)
    {
        var result = await _pipelineAppService.GetByPostIdAsync(postId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{postId:guid}/versions")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVersions(Guid postId, CancellationToken cancellationToken)
    {
        var result = await _pipelineAppService.GetVersionsAsync(postId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("versions/{versionId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVersionById(Guid versionId, CancellationToken cancellationToken)
    {
        var result = await _pipelineAppService.GetVersionByIdAsync(versionId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{postId:guid}/versions")]
    [Authorize]
    public async Task<IActionResult> CreateVersion(
        Guid postId,
        [FromBody] CreatePipelineVersionRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _pipelineAppService.CreateVersionAsync(postId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{postId:guid}/metadata")]
    [Authorize]
    public async Task<IActionResult> UpdateMetadata(
        Guid postId,
        [FromBody] UpdatePipelineMetadataRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _pipelineAppService.UpdateMetadataAsync(postId, request, cancellationToken);
        return Ok(result);
    }
}