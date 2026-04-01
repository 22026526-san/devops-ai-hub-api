namespace DevOpsAiHub.Application.Features.App.Comments.DTOs;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorUsername { get; set; } = null!;
    public string? AuthorAvatarUrl { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = null!;
    public string? ImgUrl { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<CommentDto> Replies { get; set; } = new();
}