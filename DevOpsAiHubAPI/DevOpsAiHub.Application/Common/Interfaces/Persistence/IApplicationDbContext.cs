using DevOpsAiHub.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DevOpsAiHub.Application.Common.Interfaces.Persistence;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<UserProfile> UserProfiles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}