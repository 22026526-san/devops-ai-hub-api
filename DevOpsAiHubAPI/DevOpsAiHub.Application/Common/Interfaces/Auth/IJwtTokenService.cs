using DevOpsAiHub.Domain.Entities.Users;

namespace DevOpsAiHub.Application.Common.Interfaces.Auth;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}