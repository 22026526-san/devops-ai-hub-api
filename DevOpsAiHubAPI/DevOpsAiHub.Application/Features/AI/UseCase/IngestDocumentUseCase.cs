using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.AI.DTOs;
using Microsoft.Extensions.Configuration;

namespace DevOpsAiHub.Application.Features.AI.UseCase;

public class IngestDocumentUseCase
{
    private readonly IVectorCollectionService _vectorService;
    private readonly IEmbeddingService _embedding;
    private readonly ITextChunkerService _chunker;
    private readonly string _collectionText;
    private readonly int _chunkSize;
    private readonly int _chunkOverlap;

    public IngestDocumentUseCase(
        IVectorCollectionService vectorService,
        IEmbeddingService embedding,
        ITextChunkerService chunker,
        IConfiguration config)
    {
        _vectorService = vectorService;
        _embedding = embedding;
        _chunker = chunker;
        _collectionText = config["Qdrant:CollectionText"]!;
        _chunkSize = int.Parse(config["Rag:ChunkSize"] ?? "1500");
        _chunkOverlap = int.Parse(config["Rag:ChunkOverlap"] ?? "300");
    }

    public async Task<IngestResponseDto> ExecuteAsync(
        IEnumerable<(string RawText, string FileName, string FileType)> files,
        CancellationToken ct = default)
    {
        var points = new List<VectorPointDto>();
        int fileCount = 0;

        foreach (var (rawText, fileName, fileType) in files)
        {
            if (string.IsNullOrWhiteSpace(rawText)) continue;
            fileCount++;

            var chunks = _chunker.Chunk(rawText, _chunkSize, _chunkOverlap).ToList();
            for (int i = 0; i < chunks.Count; i++)
            {
                var vec = await _embedding.EmbedAsync(chunks[i], ct);
                points.Add(new VectorPointDto(
                    Id: Guid.NewGuid().ToString("N"),
                    Vector: vec.ToArray(),
                    CollectionType: VectorCollectionType.Article,
                    ArticlePayload: new ArticlePayloadDto(
                        Source: "local",
                        Title: Path.GetFileNameWithoutExtension(fileName),
                        SourceFile: fileName,
                        ChunkIndex: i,
                        Content: chunks[i],
                        FileType: fileType
                    )
                ));
            }
        }

        if (points.Count == 0)
            throw new InvalidOperationException("No extractable text found.");

        await _vectorService.UpsertAsync(_collectionText, points, ct);

        return new IngestResponseDto(points.Count, fileCount, "Ingested successfully.");
    }
}