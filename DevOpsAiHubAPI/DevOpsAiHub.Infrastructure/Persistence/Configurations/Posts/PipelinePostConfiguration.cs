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
        builder.Property(x => x.Platform).HasColumnName("platform").HasMaxLength(50).IsRequired();
        builder.Property(x => x.PipelineFormat).HasColumnName("pipeline_format").HasMaxLength(50).IsRequired();
        builder.Property(x => x.ProjectType).HasColumnName("project_type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.EnvironmentType).HasColumnName("environment_type").HasMaxLength(50);
        builder.Property(x => x.DeploymentTarget).HasColumnName("deployment_target").HasMaxLength(100);
        builder.Property(x => x.CiEnabled).HasColumnName("ci_enabled").IsRequired();
        builder.Property(x => x.CdEnabled).HasColumnName("cd_enabled").IsRequired();
        builder.Property(x => x.TestEnabled).HasColumnName("test_enabled").IsRequired();
        builder.Property(x => x.SecurityScanEnabled).HasColumnName("security_scan_enabled").IsRequired();
        builder.Property(x => x.ForkCount).HasColumnName("fork_count").HasDefaultValue(0).IsRequired();
        builder.Property(x => x.VersionCount).HasColumnName("version_count").HasDefaultValue(0).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").IsRequired();

        builder.HasIndex(x => x.SourcePostId).HasDatabaseName("ix_pipeline_posts_source_post_id");
        builder.HasIndex(x => x.Platform).HasDatabaseName("ix_pipeline_posts_platform");
        builder.HasIndex(x => x.ProjectType).HasDatabaseName("ix_pipeline_posts_project_type");

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