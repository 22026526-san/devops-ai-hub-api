using DevOpsAiHub.Application.Features.App.Posts.DTOs;
using DevOpsAiHub.Application.Features.App.Posts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsAiHub.Api.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly IPostAppService _postAppService;

    public PostsController(IPostAppService postAppService)
    {
        _postAppService = postAppService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _postAppService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _postAppService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("question")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateQuestion(
        [FromForm] CreateQuestionPostRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _postAppService.CreateQuestionPostAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("pipeline")]
    [Authorize]
    public async Task<IActionResult> CreatePipeline(
        [FromBody] CreatePipelinePostRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _postAppService.CreatePipelinePostAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromForm] UpdatePostRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _postAppService.UpdatePostAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _postAppService.DeletePostAsync(id, cancellationToken);
        return Ok(new { message = "Post deleted successfully." });
    }
}