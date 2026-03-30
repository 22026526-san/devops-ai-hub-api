using DevOpsAiHub.Domain.Entities.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsAiHub.Infrastructure.Persistence.Configurations.Posts;

public class LikeConfiguration : IEntityTypeConfiguration<Like>
{
    public void Configure(EntityTypeBuilder<Like> builder)
    {
        builder.ToTable("likes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.PostId).HasColumnName("post_id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").IsRequired();

        builder.HasIndex(x => new { x.PostId, x.UserId })
            .IsUnique()
            .HasDatabaseName("uq_likes_post_user");

        builder.HasIndex(x => x.PostId).HasDatabaseName("ix_likes_post_id");
        builder.HasIndex(x => x.UserId).HasDatabaseName("ix_likes_user_id");

        builder.HasOne(x => x.Post)
            .WithMany(x => x.Likes)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Likes)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}