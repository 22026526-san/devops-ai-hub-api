using DevOpsAiHub.Domain.Entities.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsAiHub.Infrastructure.Persistence.Configurations.Posts;

public class PipelinePostConfiguration : IEntityTypeConfiguration<PipelinePost>
{
    public void Configure(EntityTypeBuilder<PipelinePost> builder)
    {
        builder.ToTable("pipeline_posts");

        builder.HasKey(x => x.PostId);

        builder.Property(x => x.PostId).HasColumnName("post_id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.SourcePostId).HasColumnName("source_post_id").HasColumnType("char(36)");
        builder.Property(x => x.CurrentVersionId).HasColumnName("current_version_id").HasColumnType("char(36)");
        builder.Property(x => x.Description).HasColumnName("description").HasColumnType("text");
        builder.Property(x => x.VersionCount).HasColumnName("version_count").HasDefaultValue(0).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").IsRequired();

        builder.HasIndex(x => x.SourcePostId).HasDatabaseName("ix_pipeline_posts_source_post_id");

        builder.HasOne(x => x.Post)
            .WithOne(x => x.PipelinePost)
            .HasForeignKey<PipelinePost>(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SourcePost)
            .WithMany()
            .HasForeignKey(x => x.SourcePostId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.CurrentVersion)
            .WithMany()
            .HasForeignKey(x => x.CurrentVersionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}