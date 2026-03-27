using DevOpsAiHub.Domain.Common;

namespace DevOpsAiHub.Domain.Entities.Posts;

public class Tag : BaseEntity
{
    public string Name { get; set; } = null!;
    public int PostCount { get; set; }
    public DateTime CreatedAt { get; set; }

    //public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}