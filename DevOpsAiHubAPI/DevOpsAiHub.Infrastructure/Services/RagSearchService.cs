namespace DevOpsAiHub.Infrastructure.Services;


using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.AI.DTOs;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

public class RagSearchService : IRagSearchService
{
    private readonly IVectorCollectionService _vectorService;
    private readonly IEmbeddingService _embedding;
    private readonly IRerankService _reranker;
    private readonly string _collectionQa;
    private readonly string _collectionText;

    public RagSearchService(
        IVectorCollectionService vectorService,
        IEmbeddingService embedding,
        IRerankService reranker,
        IConfiguration config)
    {
        _vectorService = vectorService;
        _embedding = embedding;
        _reranker = reranker;
        _collectionQa = config["Qdrant:CollectionQA"]!;
        _collectionText = config["Qdrant:CollectionText"]!;
    }

    public async Task<RankedContextDto> SearchAndRerankAsync(
        string query,
        int topKQa,
        int topKText,
        float minScore,
        CancellationToken ct = default)
    {
        // 1. Embed query 1 lần
        var queryVector = await _embedding.EmbedAsync(query, ct);

        // 2. Search song song 2 collection (overfetch để sau lọc)
        var (qaRaw, articleRaw) = await SearchBothAsync(
            queryVector, topKQa, topKText, ct);

        // 3. Filter theo minScore
        var qaHits = qaRaw.Where(h => h.Score >= minScore).ToList();
        var articleHits = articleRaw.Where(h => h.Score >= minScore).ToList();

        // 4. Gộp tất cả → Deduplicate
        var allHits = DeduplicateHits(qaHits.Concat(articleHits));

        // 5. Rerank bằng ONNX
        var reranked = await _reranker.RerankAsync(query, allHits, ct);

        return new RankedContextDto(qaHits, articleHits, reranked);
    }

    private async Task<(IReadOnlyList<VectorSearchResultDto> Qa,
                         IReadOnlyList<VectorSearchResultDto> Article)>
        SearchBothAsync(
            ReadOnlyMemory<float> vec,
            int topKQa, int topKText,
            CancellationToken ct)
    {
        var qaTask = _vectorService.SearchAsync(_collectionQa, vec, topKQa, VectorCollectionType.QA, ct);
        var articleTask = _vectorService.SearchAsync(_collectionText, vec, topKText, VectorCollectionType.Article, ct);

        await Task.WhenAll(qaTask, articleTask);
        return (await qaTask, await articleTask);
    }

    private static List<VectorSearchResultDto> DeduplicateHits(
        IEnumerable<VectorSearchResultDto> hits)
    {
        var seen = new HashSet<string>();
        var result = new List<VectorSearchResultDto>();

        foreach (var h in hits.OrderByDescending(x => x.Score))
        {
            var fp = Fingerprint(h.Text);
            if (seen.Add(fp))
                result.Add(h);
        }

        return result;
    }

    private static string Fingerprint(string s)
    {
        var bytes = Encoding.UTF8.GetBytes(s.Trim().ToLowerInvariant());
        return Convert.ToHexString(SHA256.HashData(bytes))[..16];
    }
}
