using DevOpsAiHub.Domain.Entities.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsAiHub.Infrastructure.Persistence.Configurations.Posts;

public class QuestionPostConfiguration : IEntityTypeConfiguration<QuestionPost>
{
    public void Configure(EntityTypeBuilder<QuestionPost> builder)
    {
        builder.ToTable("question_posts");

        builder.HasKey(x => x.PostId);

        builder.Property(x => x.PostId).HasColumnName("post_id").HasColumnType("char(36)").IsRequired();
        builder.Property(x => x.Content).HasColumnName("content").HasColumnType("longtext").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").IsRequired();

        builder.HasOne(x => x.Post)
            .WithOne(x => x.QuestionPost)
            .HasForeignKey<QuestionPost>(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}