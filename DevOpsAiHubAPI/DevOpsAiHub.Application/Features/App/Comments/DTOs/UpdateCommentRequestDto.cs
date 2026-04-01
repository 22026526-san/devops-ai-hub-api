using Microsoft.AspNetCore.Http;

namespace DevOpsAiHub.Application.Features.App.Comments.DTOs;

public class UpdateCommentRequestDto
{
    public string Content { get; set; } = null!;
    public IFormFile? Image { get; set; }
}