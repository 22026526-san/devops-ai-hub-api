using DevOpsAiHub.Application.Features.App.Comments.DTOs;
using DevOpsAiHub.Application.Features.App.Comments.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsAiHub.Api.Controllers;

[ApiController]
public class CommentsController : ControllerBase
{
    private readonly ICommentAppService _commentAppService;

    public CommentsController(ICommentAppService commentAppService)
    {
        _commentAppService = commentAppService;
    }

    [HttpGet("api/posts/{postId:guid}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByPostId(Guid postId, CancellationToken cancellationToken)
    {
        var result = await _commentAppService.GetByPostIdAsync(postId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("api/comments")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(
        [FromForm] CreateCommentRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _commentAppService.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("api/comments/{commentId:guid}/reply")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Reply(
        Guid commentId,
        [FromForm] ReplyCommentRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _commentAppService.ReplyAsync(commentId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("api/comments/{id:guid}")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromForm] UpdateCommentRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _commentAppService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("api/comments/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _commentAppService.DeleteAsync(id, cancellationToken);
        return Ok(new { message = "Comment deleted successfully." });
    }
}