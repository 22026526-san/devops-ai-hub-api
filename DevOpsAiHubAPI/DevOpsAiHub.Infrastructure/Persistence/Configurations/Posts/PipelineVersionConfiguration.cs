using DevOpsAiHub.Domain.Entities.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsAiHub.Infrastructure.Persistence.Configurations.Posts;

public class PipelineVersionConfiguration : IEntityTypeConfiguration<PipelineVersion>
{
    public void Configure(EntityTypeBuilder<PipelineVersion> builder)
    {
        builder.ToTable("pipeline_versions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.PipelinePostId).HasColumnName("pipeline_post_id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.VersionNumber).HasColumnName("version_number").IsRequired();
        builder.Property(x => x.Content).HasColumnName("content").HasColumnType("longtext").IsRequired();
        builder.Property(x => x.Changelog).HasColumnName("changelog").HasColumnType("text");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").IsRequired();

        builder.HasIndex(x => new { x.PipelinePostId, x.VersionNumber })
            .IsUnique()
            .HasDatabaseName("uq_pipeline_versions_pipeline_version");

        builder.HasIndex(x => x.PipelinePostId).HasDatabaseName("ix_pipeline_versions_pipeline_post_id");
        builder.HasIndex(x => x.CreatedBy).HasDatabaseName("ix_pipeline_versions_created_by");
        builder.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_pipeline_versions_created_at");

        builder.HasOne(x => x.PipelinePost)
            .WithMany(x => x.Versions)
            .HasForeignKey(x => x.PipelinePostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Creator)
            .WithMany(x => x.PipelineVersions)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}