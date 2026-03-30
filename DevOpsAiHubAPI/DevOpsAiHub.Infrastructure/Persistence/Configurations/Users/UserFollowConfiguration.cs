using DevOpsAiHub.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsAiHub.Infrastructure.Persistence.Configurations.Users;

public class UserFollowConfiguration : IEntityTypeConfiguration<UserFollow>
{
    public void Configure(EntityTypeBuilder<UserFollow> builder)
    {
        builder.ToTable("user_follows");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(x => x.FollowerId)
            .HasColumnName("follower_id")
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(x => x.FollowingId)
            .HasColumnName("following_id")
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.HasIndex(x => new { x.FollowerId, x.FollowingId })
            .IsUnique()
            .HasDatabaseName("uq_user_follows");

        builder.HasIndex(x => x.FollowerId)
            .HasDatabaseName("ix_user_follows_follower_id");

        builder.HasIndex(x => x.FollowingId)
            .HasDatabaseName("ix_user_follows_following_id");

        builder.HasOne(x => x.Follower)
            .WithMany(x => x.FollowingUsers)
            .HasForeignKey(x => x.FollowerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Following)
            .WithMany(x => x.Followers)
            .HasForeignKey(x => x.FollowingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}