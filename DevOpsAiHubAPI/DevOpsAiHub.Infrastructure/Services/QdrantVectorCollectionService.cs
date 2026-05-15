namespace DevOpsAiHub.Infrastructure.Services;

using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.AI.DTOs;
using Microsoft.Extensions.Configuration;


using System.Net.Http.Json;
using System.Text.Json;

/// <summary>
/// Qdrant REST thuần - không dùng IMemoryStore (experimental/deprecated)
/// Lý do: payload QA + Article có schema riêng, cần full control
/// </summary>
public class QdrantVectorCollectionService : IVectorCollectionService
{
    private readonly HttpClient _http;

    public QdrantVectorCollectionService(
        IHttpClientFactory factory,
        IConfiguration config)
    {
        _http = factory.CreateClient("Qdrant");
        _http.BaseAddress = new Uri(config["Qdrant:BaseUrl"]!);
    }

    // ── Upsert points ─────────────────────────────────────────────────────────
    public async Task UpsertAsync(
        string collectionName,
        IEnumerable<VectorPointDto> points,
        CancellationToken ct = default)
    {
        var pointList = points.ToList();
        if (pointList.Count == 0) return;

        // Đảm bảo collection tồn tại
        await EnsureCollectionAsync(collectionName, pointList[0].Vector.Length, ct);

        var payload = new
        {
            points = pointList.Select(p => new
            {
                id = p.Id,
                vector = p.Vector,
                payload = BuildPayload(p)
            })
        };

        var resp = await _http.PutAsJsonAsync(
            $"collections/{collectionName}/points?wait=true", payload, ct);
        resp.EnsureSuccessStatusCode();
    }

    // ── Search ────────────────────────────────────────────────────────────────
    public async Task<IReadOnlyList<VectorSearchResultDto>> SearchAsync(
        string collectionName,
        ReadOnlyMemory<float> queryVector,
        int topK,
        VectorCollectionType collectionType,
        CancellationToken ct = default)
    {
        var body = new
        {
            vector = queryVector.ToArray(),
            limit = topK,
            with_payload = true
        };

        var resp = await _http.PostAsJsonAsync(
            $"collections/{collectionName}/points/search", body, ct);
        resp.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(
            await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

        return doc.RootElement
            .GetProperty("result")
            .EnumerateArray()
            .Select(h => ParseHit(h, collectionType))
            .ToList();
    }

    // ── Ensure collection tồn tại ────────────────────────────────────────────
    private async Task EnsureCollectionAsync(
        string name, int dim, CancellationToken ct)
    {
        var check = await _http.GetAsync($"collections/{name}", ct);
        if (check.IsSuccessStatusCode) return;

        var body = new { vectors = new { size = dim, distance = "Cosine" } };
        var resp = await _http.PutAsJsonAsync($"collections/{name}", body, ct);
        resp.EnsureSuccessStatusCode();
    }

    // ── Build payload theo loại ───────────────────────────────────────────────
    private static object BuildPayload(VectorPointDto p)
    {
        if (p.CollectionType == VectorCollectionType.QA && p.QaPayload is { } qa)
            return new
            {
                type = "qa",
                source = qa.Source,
                question_id = qa.QuestionId,
                chunk_index = qa.ChunkIndex,
                text_content = qa.TextContent,
                question_title = qa.QuestionTitle,
                tags = qa.Tags,
                primary_tag = qa.PrimaryTag,
                url = qa.Url,
                question_score = qa.QuestionScore,
                answer_score = qa.AnswerScore,
                view_count = qa.ViewCount,
                creation_date = qa.CreationDate
            };

        if (p.ArticlePayload is { } art)
            return new
            {
                type = "article",
                source = art.Source,
                title = art.Title,
                source_file = art.SourceFile,
                chunk_index = art.ChunkIndex,
                content = art.Content,
                file_type = art.FileType
            };

        throw new InvalidOperationException("Missing payload on VectorPointDto.");
    }

    // ── Parse hit từ Qdrant response ──────────────────────────────────────────
    private static VectorSearchResultDto ParseHit(
        JsonElement h, VectorCollectionType collectionType)
    {
        var id = h.GetProperty("id").GetString() ?? "";
        var score = h.GetProperty("score").GetSingle();
        var pl = h.GetProperty("payload");

        if (collectionType == VectorCollectionType.QA)
        {
            var tags = pl.TryGetProperty("tags", out var tagsEl)
                ? tagsEl.EnumerateArray().Select(x => x.GetString() ?? "").ToArray()
                : Array.Empty<string>();

            var textContent = Str(pl, "text_content");
            var qa = new QaPayloadDto(
                Source: Str(pl, "source"),
                QuestionId: pl.TryGetProperty("question_id", out var qid) ? qid.GetInt64() : 0,
                ChunkIndex: pl.TryGetProperty("chunk_index", out var ci) ? ci.GetInt32() : 0,
                TextContent: textContent,
                QuestionTitle: Str(pl, "question_title"),
                Tags: tags,
                PrimaryTag: Str(pl, "primary_tag"),
                Url: Str(pl, "url"),
                QuestionScore: pl.TryGetProperty("question_score", out var qs) ? qs.GetInt32() : 0,
                AnswerScore: pl.TryGetProperty("answer_score", out var ans) ? ans.GetInt32() : 0,
                ViewCount: pl.TryGetProperty("view_count", out var vc) ? vc.GetInt32() : 0,
                CreationDate: Str(pl, "creation_date")
            );

            return new VectorSearchResultDto(
                id, score, textContent,
                VectorCollectionType.QA, QaPayload: qa);
        }
        else
        {
            var content = Str(pl, "content");
            var art = new ArticlePayloadDto(
                Source: Str(pl, "source"),
                Title: Str(pl, "title"),
                SourceFile: Str(pl, "source_file"),
                ChunkIndex: pl.TryGetProperty("chunk_index", out var ci) ? ci.GetInt32() : 0,
                Content: content,
                FileType: Str(pl, "file_type")
            );

            return new VectorSearchResultDto(
                id, score, content,
                VectorCollectionType.Article, ArticlePayload: art);
        }
    }

    private static string Str(JsonElement el, string key)
        => el.TryGetProperty(key, out var v) ? v.GetString() ?? "" : "";
}
