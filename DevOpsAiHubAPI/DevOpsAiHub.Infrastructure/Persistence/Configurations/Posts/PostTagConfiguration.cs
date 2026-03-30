using DevOpsAiHub.Domain.Entities.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsAiHub.Infrastructure.Persistence.Configurations.Posts;

public class PostTagConfiguration : IEntityTypeConfiguration<PostTag>
{
    public void Configure(EntityTypeBuilder<PostTag> builder)
    {
        builder.ToTable("post_tags");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.PostId).HasColumnName("post_id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.TagId).HasColumnName("tag_id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").IsRequired();

        builder.HasIndex(x => new { x.PostId, x.TagId })
            .IsUnique()
            .HasDatabaseName("uq_post_tags_post_tag");

        builder.HasIndex(x => x.PostId).HasDatabaseName("ix_post_tags_post_id");
        builder.HasIndex(x => x.TagId).HasDatabaseName("ix_post_tags_tag_id");

        builder.HasOne(x => x.Post)
            .WithMany(x => x.PostTags)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Tag)
            .WithMany(x => x.PostTags)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}