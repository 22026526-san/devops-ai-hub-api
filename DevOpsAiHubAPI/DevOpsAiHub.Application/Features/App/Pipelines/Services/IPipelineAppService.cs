using DevOpsAiHub.Application.Features.App.Pipelines.DTOs;

namespace DevOpsAiHub.Application.Features.Pipelines.Services;

public interface IPipelineAppService
{
    Task<PipelineDto> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<List<PipelineVersionDto>> GetVersionsAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<PipelineVersionDto> GetVersionByIdAsync(Guid versionId, CancellationToken cancellationToken = default);
    Task<PipelineVersionDto> CreateVersionAsync(Guid postId, CreatePipelineVersionRequestDto request, CancellationToken cancellationToken = default);
    Task<PipelineDto> UpdateMetadataAsync(Guid postId, UpdatePipelineMetadataRequestDto request, CancellationToken cancellationToken = default);
}