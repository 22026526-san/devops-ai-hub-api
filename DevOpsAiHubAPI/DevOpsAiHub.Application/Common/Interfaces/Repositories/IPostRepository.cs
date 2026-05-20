using DevOpsAiHub.Application.Features.App.Posts.DTOs;
using DevOpsAiHub.Domain.Entities.Posts;
using Microsoft.EntityFrameworkCore;

namespace DevOpsAiHub.Application.Common.Interfaces.Repositories;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Post?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    IQueryable<Post> Query();
    Task AddAsync(Post post, CancellationToken cancellationToken = default);
    void Update(Post post);
}