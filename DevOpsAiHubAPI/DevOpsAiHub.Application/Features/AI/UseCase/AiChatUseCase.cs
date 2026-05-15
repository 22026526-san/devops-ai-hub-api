using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.AI.DTOs;
using DevOpsAiHub.Domain.Entities.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DevOpsAiHub.Application.Features.AI.UseCase;

public class AiChatUseCase
{
    private readonly IRagSearchService _ragSearch;
    private readonly ILlmService _llm;
    private readonly IAiConversationRepository _convRepo;
    private readonly IAiMessageRepository _msgRepo;
    private readonly IApplicationDbContext _db;
    private readonly int _topKQa;
    private readonly int _topKText;
    private readonly float _minScore;

    public AiChatUseCase(
        IRagSearchService ragSearch,
        ILlmService llm,
        IAiConversationRepository convRepo,
        IAiMessageRepository msgRepo,
        IApplicationDbContext db,
        IConfiguration config)
    {
        _ragSearch = ragSearch;
        _llm = llm;
        _convRepo = convRepo;
        _msgRepo = msgRepo;
        _db = db;
        _topKQa = int.Parse(config["Rag:TopK_QA"] ?? "5");
        _topKText = int.Parse(config["Rag:TopK_Text"] ?? "5");
        _minScore = float.Parse(config["Rag:MinScore"] ?? "0.5");
    }

    public async Task<AiChatResponseDto> ExecuteAsync(
        AiChatRequestDto request, CancellationToken ct = default)
    {
        // 1. Tạo hoặc lấy conversation
        var conv = await GetOrCreateConversationAsync(request, ct);

        // 2. Lưu user message
        _msgRepo.Add(new AiMessage
        {
            ConversationId = conv.Id,
            Role = "user",
            Content = request.Message,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);

        // 3. Lấy lịch sử hội thoại (trừ message vừa thêm)
        var history = (await _msgRepo.GetByConversationIdAsync(conv.Id, ct))
            .SkipLast(1)
            .Select(m => new ChatMessageDto(m.Role, m.Content))
            .ToList();

        // 4. Search song song 2 collection → Rerank ONNX
        var ranked = await _ragSearch.SearchAndRerankAsync(
            request.Message,
            request.TopKQa ?? _topKQa,
            request.TopKText ?? _topKText,
            _minScore, ct);

        // 6. Build context prompt
        var (systemPrompt, userPrompt) = ContextPromptBuilder.Build(request.Message, ranked);

        // 7. Gọi LLM với history
        var reply = await _llm.ChatWithHistoryAsync(systemPrompt, history, userPrompt, ct);


        // 9. Lưu assistant message
        _msgRepo.Add(new AiMessage
        {
            ConversationId = conv.Id,
            Role = "assistant",
            Content = reply,
            CreatedAt = DateTime.UtcNow
        });

        // 10. Cập nhật conversation
        conv.UpdatedAt = DateTime.UtcNow;
        conv.Status = "active";
        _convRepo.Update(conv);

        await _db.SaveChangesAsync(ct);

        // 11. Build citations
        var sources = ranked.MergedRanked.Select((r, i) => new SourceCitationDto(
            Ordinal: i + 1,
            Label: BuildLabel(r.Hit),
            Url: r.Hit.QaPayload?.Url,
            VectorScore: r.VectorScore,
            RerankScore: r.RerankScore,
            CollectionType: r.Hit.CollectionType
        )).ToList();

        return new AiChatResponseDto(reply, conv.Id, sources);
    }

    private static string BuildLabel(VectorSearchResultDto h) =>
        h.CollectionType == VectorCollectionType.QA && h.QaPayload is { } qa
            ? $"[SO] {qa.QuestionTitle}"
            : h.ArticlePayload is { } art
                ? $"[Doc] {art.Title} ({art.FileType})"
                : h.Id;

    private async Task<AiConversation> GetOrCreateConversationAsync(
        AiChatRequestDto req, CancellationToken ct)
    {
        if (req.ConversationId.HasValue)
        {
            var existing = await _convRepo.GetByIdAsync(req.ConversationId.Value, ct);
            if (existing is not null) return existing;
        }

        return await _convRepo.CreateAsync(new AiConversation
        {
            UserId = req.UserId,
            ConversationType = "ai_chat",
            Title = req.Message.Length > 60
                                   ? req.Message[..60] + "…" : req.Message,
            Status = "active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, ct);
    }
}