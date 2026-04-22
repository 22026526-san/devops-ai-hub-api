using DevOpsAiHub.Domain.Common;
using DevOpsAiHub.Domain.Entities.Users;

namespace DevOpsAiHub.Domain.Entities.Posts;

public class Post : BaseAuditableEntity
{
    public Guid AuthorId { get; set; }
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
    public DateTime? DeletedAt { get; set; }

    public User Author { get; set; } = null!;
    public QuestionPost? QuestionPost { get; set; }
    public PipelinePost? PipelinePost { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}