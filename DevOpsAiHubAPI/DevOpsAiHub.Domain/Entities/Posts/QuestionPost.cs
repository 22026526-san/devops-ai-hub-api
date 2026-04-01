namespace DevOpsAiHub.Domain.Entities.Posts;

public class QuestionPost
{
    public Guid PostId { get; set; }
    public string Content { get; set; } = null!;
    public string? ImgUrl { get; set; }
    public string? ImgPublicId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Post Post { get; set; } = null!;
}