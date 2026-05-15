using DevOpsAiHub.Application.Common.Models;
using DevOpsAiHub.Application.Features.Users.DTOs;
using Microsoft.AspNetCore.Http;

namespace DevOpsAiHub.Application.Features.Users.Services;

public interface IUserAppService
{
    Task<UserProfileDto> GetMyProfileAsync(CancellationToken cancellationToken = default);
    Task<UserProfileDto> GetUserProfileByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResult<UserProfileDto>> GetAllProfilesAsync(GetUserQueryDto request ,CancellationToken cancellationToken = default);
    Task<List<UserProfileDto>> GetSuggestedProfilesAsync(CancellationToken cancellationToken = default);
    Task UpdateProfileAsync(UpdateProfileRequestDto request, CancellationToken cancellationToken = default);
    Task<UpdateAvatarResponseDto> UpdateAvatarAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task RemoveAvatarAsync(CancellationToken cancellationToken = default);
    Task UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequestDto request, CancellationToken cancellationToken = default);
    Task UpdateUserStatusAsync(Guid userId, UpdateUserStatusRequestDto request, CancellationToken cancellationToken = default);
    Task UpdateUserOtpRequestAsync(UpdateUserOtpRequestDto request, CancellationToken cancellationToken = default);
    Task VerifyUpdateUserOtpAsync(VerifyUpdateUserOtpDto request, CancellationToken cancellationToken = default);
}