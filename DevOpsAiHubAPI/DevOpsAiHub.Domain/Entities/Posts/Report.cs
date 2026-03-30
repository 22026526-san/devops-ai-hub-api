using DevOpsAiHub.Domain.Common;
using DevOpsAiHub.Domain.Entities.Users;

namespace DevOpsAiHub.Domain.Entities.Posts;

public class Report : BaseEntity
{
    public Guid ReporterId { get; set; }
    public Guid PostId { get; set; }
    public string Reason { get; set; } = null!;
    public string? Description { get; set; }
    public string Status { get; set; } = null!;
    public Guid? ReviewedBy { get; set; }
    public string? ReviewNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public User Reporter { get; set; } = null!;
    public Post Post { get; set; } = null!;
    public User? Reviewer { get; set; }
}