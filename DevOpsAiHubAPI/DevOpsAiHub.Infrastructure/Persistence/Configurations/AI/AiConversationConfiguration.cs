using DevOpsAiHub.Domain.Entities.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsAiHub.Infrastructure.Persistence.Configurations.AI;

public class AiConversationConfiguration : IEntityTypeConfiguration<AiConversation>
{
    public void Configure(EntityTypeBuilder<AiConversation> builder)
    {
        builder.ToTable("ai_conversations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.ConversationType).HasColumnName("conversation_type").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(255);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(50);

        builder.HasIndex(x => x.UserId).HasDatabaseName("ix_ai_conversations_user_id");
        builder.HasIndex(x => x.ConversationType).HasDatabaseName("ix_ai_conversations_type");
        builder.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_ai_conversations_created_at");

        builder.HasOne(x => x.User)
            .WithMany(x => x.AiConversations)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}