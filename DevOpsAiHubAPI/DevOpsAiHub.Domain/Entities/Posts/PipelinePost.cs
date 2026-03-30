namespace DevOpsAiHub.Domain.Entities.Posts;

public class PipelinePost
{
    public Guid PostId { get; set; }
    public Guid? SourcePostId { get; set; }
    public Guid? CurrentVersionId { get; set; }
    public string? Description { get; set; }
    public string Platform { get; set; } = null!;
    public string PipelineFormat { get; set; } = null!;
    public string ProjectType { get; set; } = null!;
    public string? EnvironmentType { get; set; }
    public string? DeploymentTarget { get; set; }
    public bool CiEnabled { get; set; }
    public bool CdEnabled { get; set; }
    public bool TestEnabled { get; set; }
    public bool SecurityScanEnabled { get; set; }
    public int ForkCount { get; set; }
    public int VersionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Post Post { get; set; } = null!;
    public Post? SourcePost { get; set; }
    public PipelineVersion? CurrentVersion { get; set; }
    public ICollection<PipelineVersion> Versions { get; set; } = new List<PipelineVersion>();
    public ICollection<AI.AiPipelineResult> AiPipelineResults { get; set; } = new List<AI.AiPipelineResult>();
}