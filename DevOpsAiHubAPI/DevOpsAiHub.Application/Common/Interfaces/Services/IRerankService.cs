using DevOpsAiHub.Application.Features.AI.DTOs;
namespace DevOpsAiHub.Application.Common.Interfaces.Services;

public interface IRerankService
{
    /// <summary>
    /// Rerank danh sách hits theo relevance với query
    /// Trả về danh sách đã sort theo FinalScore desc
    /// </summary>
    Task<IReadOnlyList<RankedHitDto>> RerankAsync(
        string query,
        IEnumerable<VectorSearchResultDto> hits,
        CancellationToken ct = default);
}