using DevOpsAiHub.Application.Common.Models;
using DevOpsAiHub.Application.Features.App.Posts.DTOs;

namespace DevOpsAiHub.Application.Features.App.Posts.Services;

public interface IPostAppService
{
    Task<PagedResult<PostDto>> GetAllAsync(GetPostsQueryDto request, CancellationToken cancellationToken = default);
    Task<PostDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PostDto> CreateQuestionPostAsync(CreateQuestionPostRequestDto request, CancellationToken cancellationToken = default);
    Task<PostDto> CreatePipelinePostAsync(CreatePipelinePostRequestDto request, CancellationToken cancellationToken = default);
    Task<PostDto> UpdatePostAsync(Guid id, UpdatePostRequestDto request, CancellationToken cancellationToken = default);
    Task DeletePostAsync(Guid id, CancellationToken cancellationToken = default);
}