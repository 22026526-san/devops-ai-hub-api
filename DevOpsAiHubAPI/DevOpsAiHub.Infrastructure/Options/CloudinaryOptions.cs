namespace DevOpsAiHub.Infrastructure.Options;

public class CloudinaryOptions
{
    public const string SectionName = "Cloudinary";

    public string CloudName { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string ApiSecret { get; set; } = null!;
    public string AvatarFolder { get; set; } = "avatars";
}