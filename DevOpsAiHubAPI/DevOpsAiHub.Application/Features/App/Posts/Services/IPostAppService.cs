using DevOpsAiHub.Application.Features.App.Posts.DTOs;

namespace DevOpsAiHub.Application.Features.App.Posts.Services;

public interface IPostAppService
{
    Task<List<PostDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PostDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PostDetailDto> CreateQuestionPostAsync(CreateQuestionPostRequestDto request, CancellationToken cancellationToken = default);
    Task<PostDetailDto> CreatePipelinePostAsync(CreatePipelinePostRequestDto request, CancellationToken cancellationToken = default);
    Task<PostDetailDto> UpdatePostAsync(Guid id, UpdatePostRequestDto request, CancellationToken cancellationToken = default);
    Task DeletePostAsync(Guid id, CancellationToken cancellationToken = default);
}