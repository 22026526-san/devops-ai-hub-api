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

        builder.HasOne(x => x.Follower)
            .WithMany()
            .HasForeignKey(x => x.FollowerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Following)
            .WithMany()
            .HasForeignKey(x => x.FollowingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}