namespace DevOpsAiHub.Application.Common.Interfaces.Services;


public interface IEmbeddingService
{
    Task<ReadOnlyMemory<float>> EmbedAsync(string text, CancellationToken ct = default);
    Task<int> GetDimensionAsync(CancellationToken ct = default);
}
