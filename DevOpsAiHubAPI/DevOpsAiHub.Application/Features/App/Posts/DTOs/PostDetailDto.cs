namespace DevOpsAiHub.Application.Features.App.Posts.DTOs;

public class PostDetailDto
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorUsername { get; set; } = null!;
    public string PostType { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Summary { get; set; }
    public string Status { get; set; } = null!;
    public string Visibility { get; set; } = null!;
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int BookmarkCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string? QuestionContent { get; set; }
    public string? QuestionImgUrl { get; set; }

    public string? PipelineDescription { get; set; }
    public string? Platform { get; set; }
    public string? PipelineFormat { get; set; }
    public string? ProjectType { get; set; }
    public string? EnvironmentType { get; set; }
    public string? DeploymentTarget { get; set; }
    public bool? CiEnabled { get; set; }
    public bool? CdEnabled { get; set; }
    public bool? TestEnabled { get; set; }
    public bool? SecurityScanEnabled { get; set; }
    public int? ForkCount { get; set; }
    public int? VersionCount { get; set; }
    public string? CurrentPipelineContent { get; set; }
}