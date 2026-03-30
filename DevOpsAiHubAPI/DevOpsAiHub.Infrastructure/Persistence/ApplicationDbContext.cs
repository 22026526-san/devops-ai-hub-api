using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Domain.Entities.AI;
using DevOpsAiHub.Domain.Entities.Posts;
using DevOpsAiHub.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace DevOpsAiHub.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserFollow> UserFollows => Set<UserFollow>();

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<QuestionPost> QuestionPosts => Set<QuestionPost>();
    public DbSet<PipelinePost> PipelinePosts => Set<PipelinePost>();
    public DbSet<PipelineVersion> PipelineVersions => Set<PipelineVersion>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<Report> Reports => Set<Report>();

    public DbSet<AiConversation> AiConversations => Set<AiConversation>();
    public DbSet<AiMessage> AiMessages => Set<AiMessage>();
    public DbSet<AiPipelineResult> AiPipelineResults => Set<AiPipelineResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}