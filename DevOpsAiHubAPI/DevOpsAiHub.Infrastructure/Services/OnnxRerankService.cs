using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.AI.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers; 

namespace DevOpsAiHub.Infrastructure.Services;

public class OnnxRerankService : IRerankService, IDisposable
{
    private readonly InferenceSession _session;
    private readonly Tokenizer _tokenizer;
    private readonly int _maxLength;

    private const int ClsTokenId = 0;
    private const int SepTokenId = 2; 

    private const float VectorWeight = 0.3f;
    private const float RerankWeight = 0.7f;

    public OnnxRerankService(IConfiguration config)
    {
        var modelDir = config["Reranker:ModelPath"]
                     ?? Path.Combine(AppContext.BaseDirectory, "Assets", "reranker");

        var modelPath = Path.Combine(modelDir, "model.onnx");

        var spModelPath = Path.Combine(modelDir, "sentencepiece.bpe.model");

        _maxLength = int.Parse(config["Reranker:MaxLength"] ?? "512");

        var sessionOptions = new SessionOptions
        {
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
            EnableMemoryPattern = true
        };

        _session = new InferenceSession(modelPath, sessionOptions);

        using var stream = File.OpenRead(spModelPath);
        _tokenizer = SentencePieceTokenizer.Create(stream);
    }

    public Task<IReadOnlyList<RankedHitDto>> RerankAsync(
        string query,
        IEnumerable<VectorSearchResultDto> hits,
        CancellationToken ct = default)
    {
        var hitList = hits.ToList();
        if (hitList.Count == 0)
            return Task.FromResult<IReadOnlyList<RankedHitDto>>(Array.Empty<RankedHitDto>());

        var ranked = hitList
            .Select(h =>
            {
                var rerankScore = ScorePair(query, h.Text);
                return new RankedHitDto(
                    Hit: h,
                    VectorScore: h.Score,
                    RerankScore: rerankScore,
                    FinalScore: VectorWeight * h.Score + RerankWeight * rerankScore
                );
            })
            .OrderByDescending(r => r.FinalScore)
            .Take(5)
            .ToList();

        return Task.FromResult<IReadOnlyList<RankedHitDto>>(ranked);
    }

    private float ScorePair(string query, string passage)
    {
        var queryIds = _tokenizer.EncodeToIds(query);
        var passageIds = _tokenizer.EncodeToIds(passage);

        const int specialTokensCount = 4;

        int maxQueryLen = Math.Min(queryIds.Count, _maxLength / 2);
        int maxPassageLen = _maxLength - maxQueryLen - specialTokensCount;

        var truncatedQuery = queryIds.Take(maxQueryLen).ToList();
        var truncatedPassage = passageIds.Take(maxPassageLen).ToList();

        var inputIdsList = new List<long>(capacity: _maxLength) { ClsTokenId };
        inputIdsList.AddRange(truncatedQuery.Select(x => (long)x));
        inputIdsList.Add(SepTokenId);
        inputIdsList.Add(SepTokenId);
        inputIdsList.AddRange(truncatedPassage.Select(x => (long)x));
        inputIdsList.Add(SepTokenId);

        long[] inputIds = inputIdsList.ToArray();
        int seqLen = inputIds.Length;

        var inputIdsTensor = new DenseTensor<long>(inputIds, new[] { 1, seqLen });
        var attentionMaskTensor = new DenseTensor<long>(new[] { 1, seqLen });

        for (int i = 0; i < seqLen; i++) attentionMaskTensor[0, i] = 1L;

        var inputs = new List<NamedOnnxValue>
    {
        NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
        NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor)
    };

        using var outputs = _session.Run(inputs);

        var outputTensor = outputs.First(v => v.Name == "logits").AsTensor<float>();

    
        float rawScore = outputTensor.First();

        return Sigmoid(rawScore);
    }

    private static float Sigmoid(float x) => 1f / (1f + MathF.Exp(-x));

    public void Dispose() => _session.Dispose();
}