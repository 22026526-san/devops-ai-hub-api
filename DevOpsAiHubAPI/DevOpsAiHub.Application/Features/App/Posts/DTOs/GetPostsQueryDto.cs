namespace DevOpsAiHub.Application.Features.App.Posts.DTOs;

public class GetPostsQueryDto
{
    public string? Search { get; set; }
    public List<Guid> TagIds { get; set; } = new();
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int? Day { get; set; }
    public string SortBy { get; set; } = "latest";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}