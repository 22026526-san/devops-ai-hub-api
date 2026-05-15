using DevOpsAiHub.Application.Common.Models;
using DevOpsAiHub.Application.Features.App.Tags.DTOs;
using DevOpsAiHub.Application.Features.Users.DTOs;

namespace DevOpsAiHub.Application.Features.App.Tags.Services;

public interface ITagAppService
{
    Task<PagedResult<TagDto>> GetAllAsync(GetTagQueryDto request, CancellationToken cancellationToken = default);
    Task<TagDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TagDto> CreateAsync(CreateTagRequestDto request, CancellationToken cancellationToken = default);
    Task<TagDto> UpdateAsync(Guid id, UpdateTagRequestDto request, CancellationToken cancellationToken = default);
}