namespace DevOpsAiHub.Domain.Entities.Posts;

public class PipelinePost
{
    public Guid PostId { get; set; }
    public Guid? SourcePostId { get; set; }
    public Guid? CurrentVersionId { get; set; }
    public string? Description { get; set; }
    public int VersionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Post Post { get; set; } = null!;
    public Post? SourcePost { get; set; }
    public PipelineVersion? CurrentVersion { get; set; }
    public ICollection<PipelineVersion> Versions { get; set; } = new List<PipelineVersion>();
}