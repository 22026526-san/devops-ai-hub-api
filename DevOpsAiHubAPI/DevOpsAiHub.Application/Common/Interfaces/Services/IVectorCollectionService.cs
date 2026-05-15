using DevOpsAiHub.Application.Features.AI.DTOs;

namespace DevOpsAiHub.Application.Common.Interfaces.Services;

public interface IVectorCollectionService
{
    Task UpsertAsync(
        string collectionName,
        IEnumerable<VectorPointDto> points,
        CancellationToken ct = default);

    Task<IReadOnlyList<VectorSearchResultDto>> SearchAsync(
        string collectionName,
        ReadOnlyMemory<float> queryVector,
        int topK,
        VectorCollectionType collectionType,
        CancellationToken ct = default);
}