using DevOpsAiHub.Domain.Entities.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsAiHub.Infrastructure.Persistence.Configurations.AI;

public class AiPipelineResultConfiguration : IEntityTypeConfiguration<AiPipelineResult>
{
    public void Configure(EntityTypeBuilder<AiPipelineResult> builder)
    {
        builder.ToTable("ai_pipeline_results");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.ConversationId).HasColumnName("conversation_id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.ResultType).HasColumnName("result_type").HasMaxLength(30).IsRequired();
        builder.Property(x => x.InputContent).HasColumnName("input_content").HasColumnType("longtext");
        builder.Property(x => x.OutputContent).HasColumnName("output_content").HasColumnType("longtext").IsRequired();
        builder.Property(x => x.Platform).HasColumnName("platform").HasMaxLength(50);
        builder.Property(x => x.ProjectType).HasColumnName("project_type").HasMaxLength(100);
        builder.Property(x => x.RelatedPipelinePostId).HasColumnName("related_pipeline_post_id").HasColumnType("char(36)");
        builder.Property(x => x.IsSavedAsPost).HasColumnName("is_saved_as_post").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").IsRequired();

        builder.HasIndex(x => x.ConversationId).HasDatabaseName("ix_ai_pipeline_results_conversation_id");
        builder.HasIndex(x => x.RelatedPipelinePostId).HasDatabaseName("ix_ai_pipeline_results_related_pipeline_post_id");
        builder.HasIndex(x => x.ResultType).HasDatabaseName("ix_ai_pipeline_results_result_type");
        builder.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_ai_pipeline_results_created_at");

        builder.HasOne(x => x.Conversation)
            .WithMany(x => x.PipelineResults)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.RelatedPipelinePost)
            .WithMany(x => x.AiPipelineResults)
            .HasForeignKey(x => x.RelatedPipelinePostId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}