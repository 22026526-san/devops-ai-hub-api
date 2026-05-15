namespace DevOpsAiHub.Application.Features.AI.DTOs;

public enum VectorCollectionType { QA, Article }

public record VectorSearchResultDto(
    string Id,
    float Score,
    string Text,
    VectorCollectionType CollectionType,
    QaPayloadDto? QaPayload = null,
    ArticlePayloadDto? ArticlePayload = null
);

public record QaPayloadDto(
    string Source,
    long QuestionId,
    int ChunkIndex,
    string TextContent,
    string QuestionTitle,
    string[] Tags,
    string PrimaryTag,
    string Url,
    int QuestionScore,
    int AnswerScore,
    int ViewCount,
    string CreationDate
);

public record ArticlePayloadDto(
    string Source,
    string Title,
    string SourceFile,
    int ChunkIndex,
    string Content,
    string FileType
);

public record VectorPointDto(
    string Id,
    float[] Vector,
    VectorCollectionType CollectionType,
    QaPayloadDto? QaPayload = null,
    ArticlePayloadDto? ArticlePayload = null
);

public record DocumentChunkDto(string Text, string? Source);