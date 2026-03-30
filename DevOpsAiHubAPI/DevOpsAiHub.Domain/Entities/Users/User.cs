using DevOpsAiHub.Domain.Common;
using DevOpsAiHub.Domain.Entities.AI;
using DevOpsAiHub.Domain.Entities.Posts;

namespace DevOpsAiHub.Domain.Entities.Users;

public class User : BaseAuditableEntity
{
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string Status { get; set; } = null!;

    public UserProfile? Profile { get; set; }

    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<PipelineVersion> PipelineVersions { get; set; } = new List<PipelineVersion>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    public ICollection<Report> ReportsCreated { get; set; } = new List<Report>();
    public ICollection<Report> ReportsReviewed { get; set; } = new List<Report>();
    public ICollection<AiConversation> AiConversations { get; set; } = new List<AiConversation>();

    public ICollection<UserFollow> Followers { get; set; } = new List<UserFollow>();
    public ICollection<UserFollow> FollowingUsers { get; set; } = new List<UserFollow>();
}