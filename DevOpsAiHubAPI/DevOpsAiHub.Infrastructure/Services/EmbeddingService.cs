using DevOpsAiHub.Application.Common.Interfaces.Services;
using Microsoft.Extensions.AI;

namespace DevOpsAiHub.Infrastructure.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;
    private int? _cachedDim;

    public EmbeddingService(
        IEmbeddingGenerator<string, Embedding<float>> generator)
    {
        _generator = generator;
    }

    public async Task<ReadOnlyMemory<float>> EmbedAsync(
        string text, CancellationToken ct = default)
    {
        var result = await _generator.GenerateAsync(
            new[] { text },
            cancellationToken: ct);

        var vec = result[0].Vector;
        _cachedDim ??= vec.Length;
        return vec;
    }

    public async Task<int> GetDimensionAsync(CancellationToken ct = default)
    {
        if (_cachedDim.HasValue) return _cachedDim.Value;
        await EmbedAsync("dimension probe", ct);
        return _cachedDim!.Value;
    }
}