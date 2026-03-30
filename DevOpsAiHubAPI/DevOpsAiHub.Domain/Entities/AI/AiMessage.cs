using DevOpsAiHub.Domain.Common;

namespace DevOpsAiHub.Domain.Entities.AI;

public class AiMessage : BaseEntity
{
    public Guid ConversationId { get; set; }
    public string Role { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public AiConversation Conversation { get; set; } = null!;
}