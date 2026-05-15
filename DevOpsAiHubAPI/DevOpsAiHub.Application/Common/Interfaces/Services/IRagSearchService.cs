namespace DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.AI.DTOs;
public interface IRagSearchService
{
    Task<RankedContextDto> SearchAndRerankAsync(
        string query,
        int topKQa,
        int topKText,
        float minScore,
        CancellationToken ct = default);
}
