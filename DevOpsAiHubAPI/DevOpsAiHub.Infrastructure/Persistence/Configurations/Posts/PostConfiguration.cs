using DevOpsAiHub.Domain.Entities.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsAiHub.Infrastructure.Persistence.Configurations.Posts;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.AuthorId).HasColumnName("author_id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.PostType).HasColumnName("post_type").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
        builder.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(255).IsRequired();
        builder.Property(x => x.Summary).HasColumnName("summary").HasColumnType("text");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Visibility).HasColumnName("visibility").HasMaxLength(20).IsRequired();
        builder.Property(x => x.ViewCount).HasColumnName("view_count").HasDefaultValue(0).IsRequired();
        builder.Property(x => x.LikeCount).HasColumnName("like_count").HasDefaultValue(0).IsRequired();
        builder.Property(x => x.CommentCount).HasColumnName("comment_count").HasDefaultValue(0).IsRequired();
        builder.Property(x => x.BookmarkCount).HasColumnName("bookmark_count").HasDefaultValue(0).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").IsRequired();
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("datetime");

        builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("uq_posts_slug");
        builder.HasIndex(x => x.AuthorId).HasDatabaseName("ix_posts_author_id");
        builder.HasIndex(x => x.PostType).HasDatabaseName("ix_posts_post_type");
        builder.HasIndex(x => x.Status).HasDatabaseName("ix_posts_status");
        builder.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_posts_created_at");

        builder.HasOne(x => x.Author)
            .WithMany(x => x.Posts)
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}