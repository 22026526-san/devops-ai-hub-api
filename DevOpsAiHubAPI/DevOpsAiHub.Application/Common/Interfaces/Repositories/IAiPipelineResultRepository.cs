using DevOpsAiHub.Domain.Entities.AI;

namespace DevOpsAiHub.Application.Common.Interfaces.Repositories;

public interface IAiPipelineResultRepository
{
    Task AddAsync(AiPipelineResult result, CancellationToken cancellationToken = default);
}