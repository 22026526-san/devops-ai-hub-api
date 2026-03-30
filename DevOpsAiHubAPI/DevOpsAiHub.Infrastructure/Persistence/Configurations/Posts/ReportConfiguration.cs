using DevOpsAiHub.Domain.Entities.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsAiHub.Infrastructure.Persistence.Configurations.Posts;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("reports");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.ReporterId).HasColumnName("reporter_id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.PostId).HasColumnName("post_id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasColumnType("text");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(x => x.ReviewedBy).HasColumnName("reviewed_by").HasColumnType("char(36)");
        builder.Property(x => x.ReviewNote).HasColumnName("review_note").HasColumnType("text");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").IsRequired();
        builder.Property(x => x.ReviewedAt).HasColumnName("reviewed_at").HasColumnType("datetime");

        builder.HasIndex(x => x.ReporterId).HasDatabaseName("ix_reports_reporter_id");
        builder.HasIndex(x => x.PostId).HasDatabaseName("ix_reports_post_id");
        builder.HasIndex(x => x.Status).HasDatabaseName("ix_reports_status");
        builder.HasIndex(x => x.ReviewedBy).HasDatabaseName("ix_reports_reviewed_by");

        builder.HasOne(x => x.Reporter)
            .WithMany(x => x.ReportsCreated)
            .HasForeignKey(x => x.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Post)
            .WithMany(x => x.Reports)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Reviewer)
            .WithMany(x => x.ReportsReviewed)
            .HasForeignKey(x => x.ReviewedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}