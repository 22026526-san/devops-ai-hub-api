using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Application.Features.AI.DTOs;
using DevOpsAiHub.Application.Features.AI.UseCase;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsAiHub.Api.Controllers;

[ApiController]
[Route("api/ai")]
public class AiChatController : ControllerBase
{
    private readonly AiChatUseCase _chatUseCase;
    private readonly IAiConversationRepository _convRepo;
    private readonly IAiMessageRepository _msgRepo;
    private readonly IApplicationDbContext _db;

    public AiChatController(
        AiChatUseCase chatUseCase,
        IAiConversationRepository convRepo,
        IAiMessageRepository msgRepo,
        IApplicationDbContext db)
    {
        _chatUseCase = chatUseCase;
        _convRepo = convRepo;
        _msgRepo = msgRepo;
        _db = db;
 
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat(
        [FromBody] AiChatRequestDto request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { error = "Message is required." });

        var result = await _chatUseCase.ExecuteAsync(request, ct);
        return Ok(result);
    }


    [HttpGet("conversations/user/{userId:guid}")]
    public async Task<IActionResult> GetConversations(
        Guid userId, CancellationToken ct)
    {
        var conversations = await _convRepo.GetByUserIdAsync(userId, ct);
        return Ok(conversations);
    }

    [HttpGet("conversations/{conversationId:guid}/messages")]
    public async Task<IActionResult> GetMessages(
        Guid conversationId, CancellationToken ct)
    {
        var messages = await _msgRepo.GetByConversationIdAsync(conversationId, ct);
        return Ok(messages);
    }

    [HttpDelete("conversations/{conversationId:guid}")]
    public async Task<IActionResult> DeleteConversation(
        Guid conversationId, CancellationToken ct)
    {
        var existing = await _convRepo.GetByIdAsync(conversationId, ct);
        if (existing is null)
            return NotFound(new { error = $"Conversation {conversationId} not found." });

        await _convRepo.DeleteAsync(conversationId, ct);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPut("conversations/{conversationId:guid}/title")]
    public async Task<IActionResult> UpdateTitle(
        Guid conversationId,
        [FromBody] UpdateConversationTitleDto request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.NewTitle))
            return BadRequest(new { error = "NewTitle is required." });

        var existing = await _convRepo.GetByIdAsync(conversationId, ct);
        if (existing is null)
            return NotFound(new { error = $"Conversation {conversationId} not found." });

        await _convRepo.UpdateTitleAsync(conversationId, request.NewTitle, ct);
        await _db.SaveChangesAsync(ct);
        return Ok(new { conversationId, title = request.NewTitle });
    }
}