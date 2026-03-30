using DevOpsAiHub.Domain.Entities.AI;
using DevOpsAiHub.Domain.Entities.Posts;
using DevOpsAiHub.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace DevOpsAiHub.Application.Common.Interfaces.Persistence;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<UserFollow> UserFollows { get; }

    DbSet<Post> Posts { get; }
    DbSet<QuestionPost> QuestionPosts { get; }
    DbSet<PipelinePost> PipelinePosts { get; }
    DbSet<PipelineVersion> PipelineVersions { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Like> Likes { get; }
    DbSet<Bookmark> Bookmarks { get; }
    DbSet<Tag> Tags { get; }
    DbSet<PostTag> PostTags { get; }
    DbSet<Report> Reports { get; }

    DbSet<AiConversation> AiConversations { get; }
    DbSet<AiMessage> AiMessages { get; }
    DbSet<AiPipelineResult> AiPipelineResults { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}