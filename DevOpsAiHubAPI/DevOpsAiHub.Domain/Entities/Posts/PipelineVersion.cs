using DevOpsAiHub.Domain.Common;
using DevOpsAiHub.Domain.Entities.Users;

namespace DevOpsAiHub.Domain.Entities.Posts;

public class PipelineVersion : BaseEntity
{
    public Guid PipelinePostId { get; set; }
    public int VersionNumber { get; set; }
    public string Content { get; set; } = null!;
    public string? Changelog { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public PipelinePost PipelinePost { get; set; } = null!;
    public User Creator { get; set; } = null!;
}