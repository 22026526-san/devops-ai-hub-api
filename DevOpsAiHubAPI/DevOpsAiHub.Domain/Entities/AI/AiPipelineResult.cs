using DevOpsAiHub.Domain.Common;
using DevOpsAiHub.Domain.Entities.Posts;

namespace DevOpsAiHub.Domain.Entities.AI;

public class AiPipelineResult : BaseEntity
{
    public Guid ConversationId { get; set; }
    public string ResultType { get; set; } = null!;
    public string? InputContent { get; set; }
    public string OutputContent { get; set; } = null!;
    public string? Platform { get; set; }
    public string? ProjectType { get; set; }
    public Guid? RelatedPipelinePostId { get; set; }
    public bool IsSavedAsPost { get; set; }
    public DateTime CreatedAt { get; set; }

    public AiConversation Conversation { get; set; } = null!;
    public PipelinePost? RelatedPipelinePost { get; set; }
}