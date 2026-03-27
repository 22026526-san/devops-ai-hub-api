using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Domain.Entities.Posts;
using DevOpsAiHub.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace DevOpsAiHub.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<UserFollow> UserFollows => Set<UserFollow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}