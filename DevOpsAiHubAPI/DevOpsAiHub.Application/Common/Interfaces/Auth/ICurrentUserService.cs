namespace DevOpsAiHub.Application.Common.Interfaces.Auth;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
}