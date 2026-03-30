using DevOpsAiHub.Domain.Common;
using DevOpsAiHub.Domain.Entities.Users;

namespace DevOpsAiHub.Domain.Entities.Posts;

public class Bookmark : BaseEntity
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Post Post { get; set; } = null!;
    public User User { get; set; } = null!;
}