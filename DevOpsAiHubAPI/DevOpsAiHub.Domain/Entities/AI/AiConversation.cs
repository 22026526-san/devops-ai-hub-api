using DevOpsAiHub.Domain.Common;
using DevOpsAiHub.Domain.Entities.Users;

namespace DevOpsAiHub.Domain.Entities.AI;

public class AiConversation : BaseEntity
{
    public Guid UserId { get; set; }
    public string ConversationType { get; set; } = null!;
    public string? Title { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<AiMessage> Messages { get; set; } = new List<AiMessage>();
}