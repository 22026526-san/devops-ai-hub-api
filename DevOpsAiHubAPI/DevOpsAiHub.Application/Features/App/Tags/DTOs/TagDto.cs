namespace DevOpsAiHub.Application.Features.App.Tags.DTOs;

public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int PostCount { get; set; }
    public DateTime CreatedAt { get; set; }
}