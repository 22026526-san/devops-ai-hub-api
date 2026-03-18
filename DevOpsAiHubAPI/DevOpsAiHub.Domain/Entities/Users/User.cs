using DevOpsAiHub.Domain.Common;

namespace DevOpsAiHub.Domain.Entities.Users;

public class User : BaseAuditableEntity
{
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string Status { get; set; } = null!;

    public UserProfile? Profile { get; set; }
}