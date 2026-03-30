using DevOpsAiHub.Domain.Entities.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsAiHub.Infrastructure.Persistence.Configurations.Posts;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.PostId).HasColumnName("post_id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.AuthorId).HasColumnName("author_id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.ParentCommentId).HasColumnName("parent_comment_id").HasColumnType("char(36)");
        builder.Property(x => x.Content).HasColumnName("content").HasColumnType("text").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").IsRequired();
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("datetime");

        builder.HasIndex(x => x.PostId).HasDatabaseName("ix_comments_post_id");
        builder.HasIndex(x => x.AuthorId).HasDatabaseName("ix_comments_author_id");
        builder.HasIndex(x => x.ParentCommentId).HasDatabaseName("ix_comments_parent_comment_id");
        builder.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_comments_created_at");

        builder.HasOne(x => x.Post)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Author)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ParentComment)
            .WithMany(x => x.Replies)
            .HasForeignKey(x => x.ParentCommentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}