namespace DevOpsAiHub.Application.Features.AI.DTOs;

public record RankedContextDto(
    IReadOnlyList<VectorSearchResultDto> QaHits,
    IReadOnlyList<VectorSearchResultDto> ArticleHits,
    IReadOnlyList<RankedHitDto> MergedRanked
);


public record RankedHitDto(
    VectorSearchResultDto Hit,
    float VectorScore,
    float RerankScore,
    float FinalScore  
);

public record RagQueryRequestDto(
    string Question,
    Guid UserId,
    Guid? ConversationId = null,
    int? TopKQa = null,
    int? TopKText = null
);

public record RagQueryResponseDto(
    string Answer,
    Guid ConversationId,
    IReadOnlyList<RankedHitDto> Sources
);

public record AiChatRequestDto(
    string Message,
    Guid UserId,
    Guid? ConversationId = null,
    int? TopKQa = null,
    int? TopKText = null
);

public record AiChatResponseDto(
    string Reply,
    Guid ConversationId,
    IReadOnlyList<SourceCitationDto> Sources
);

public record SourceCitationDto(
    int Ordinal,
    string Label,
    string? Url,
    float VectorScore,
    float RerankScore,
    VectorCollectionType CollectionType
);

public record ChatMessageDto(string Role, string Content);

public record IngestResponseDto(int IngestedChunks, int FileCount, string Message);