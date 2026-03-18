using DevOpsAiHub.Application.Features.Users.DTOs;
using Microsoft.AspNetCore.Http;

namespace DevOpsAiHub.Application.Features.Users.Services;

public interface IUserAppService
{
    Task<UserProfileDto> GetMyProfileAsync(CancellationToken cancellationToken = default);
    Task UpdateProfileAsync(UpdateProfileRequestDto request, CancellationToken cancellationToken = default);
    Task<UpdateAvatarResponseDto> UpdateAvatarAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task RemoveAvatarAsync(CancellationToken cancellationToken = default);
}