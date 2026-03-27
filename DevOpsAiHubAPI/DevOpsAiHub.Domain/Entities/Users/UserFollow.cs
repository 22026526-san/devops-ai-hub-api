using DevOpsAiHub.Domain.Common;

namespace DevOpsAiHub.Domain.Entities.Users;

public class UserFollow : BaseEntity
{
    public Guid FollowerId { get; set; }
    public Guid FollowingId { get; set; }
    public DateTime CreatedAt { get; set; }

    public User Follower { get; set; } = null!;
    public User Following { get; set; } = null!;
}