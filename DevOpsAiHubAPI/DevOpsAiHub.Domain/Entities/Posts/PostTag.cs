using DevOpsAiHub.Domain.Common;

namespace DevOpsAiHub.Domain.Entities.Posts;

public class PostTag : BaseEntity
{
    public Guid PostId { get; set; }
    public Guid TagId { get; set; }
    public DateTime CreatedAt { get; set; }

    //public Post Post { get; set; } = null!;
    //public Tag Tag { get; set; } = null!;
}