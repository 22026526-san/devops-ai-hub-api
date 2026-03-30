namespace DevOpsAiHub.Application.Features.App.Posts.DTOs;

public class PostDto
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorUsername { get; set; } = null!;
    public string PostType { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Summary { get; set; }
    public string Status { get; set; } = null!;
    public string Visibility { get; set; } = null!;
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int BookmarkCount { get; set; }
    public DateTime CreatedAt { get; set; }
}