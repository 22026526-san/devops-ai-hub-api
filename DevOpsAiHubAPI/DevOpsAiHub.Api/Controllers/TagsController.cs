using DevOpsAiHub.Application.Features.App.Tags.DTOs;
using DevOpsAiHub.Application.Features.App.Tags.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsAiHub.Api.Controllers;

[ApiController]
[Route("api/tags")]
public class TagsController : ControllerBase
{
    private readonly ITagAppService _tagAppService;

    public TagsController(ITagAppService tagAppService)
    {
        _tagAppService = tagAppService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _tagAppService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _tagAppService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] CreateTagRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _tagAppService.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTagRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _tagAppService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }
}