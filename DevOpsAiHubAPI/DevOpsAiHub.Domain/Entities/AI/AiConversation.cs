using DevOpsAiHub.Domain.Common;
using DevOpsAiHub.Domain.Entities.Posts;
using DevOpsAiHub.Domain.Entities.Users;
using System.Net.Mail;

namespace DevOpsAiHub.Domain.Entities.AI;

public class AiConversation : BaseEntity
{
    public Guid UserId { get; set; }
    public string ConversationType { get; set; } = null!;
    public string? Title { get; set; }
    public Guid? RelatedPostId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Post? RelatedPost { get; set; }
    public ICollection<AiMessage> Messages { get; set; } = new List<AiMessage>();
    public ICollection<AiPipelineResult> PipelineResults { get; set; } = new List<AiPipelineResult>();
}