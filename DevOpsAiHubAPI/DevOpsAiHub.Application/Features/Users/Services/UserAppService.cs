using DevOpsAiHub.Application.Common.Exceptions;
using DevOpsAiHub.Application.Common.Interfaces.Auth;
using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.Users.DTOs;
using Microsoft.AspNetCore.Http;

namespace DevOpsAiHub.Application.Features.Users.Services;

public class UserAppService : IUserAppService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserFollowRepository _userFollowRepository;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IApplicationDbContext _context;

    public UserAppService(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IUserProfileRepository userProfileRepository,
        IUserFollowRepository userFollowRepository,
        ICloudinaryService cloudinaryService,
        IDateTimeService dateTimeService,
        IApplicationDbContext context)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _userProfileRepository = userProfileRepository;
        _userFollowRepository = userFollowRepository;
        _cloudinaryService = cloudinaryService;
        _dateTimeService = dateTimeService;
        _context = context;
    }

    public async Task<UserProfileDto> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User is not authenticated.");

        return await BuildUserProfileDtoAsync(userId.Value, cancellationToken);
    }

    public async Task<UserProfileDto> GetUserProfileByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await BuildUserProfileDtoAsync(userId, cancellationToken);
    }
    public async Task<List<UserProfileDto>> GetAllProfilesAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        var userIds = users.Select(x => x.Id).ToList();
        var followerMap = await _userFollowRepository.CountFollowersByUserIdsAsync(userIds, cancellationToken);

        return users.Select(user =>
        {
            var profile = user.Profile;
            followerMap.TryGetValue(user.Id, out var followerCount);

            return new UserProfileDto
            {
                UserId = user.Id,
                Email = user.Email,
                Username = user.Username,
                FullName = profile?.FullName,
                AvatarUrl = profile?.AvatarUrl,
                Bio = profile?.Bio,
                GithubUrl = profile?.GithubUrl,
                FollowerCount = followerCount
            };
        }).ToList();
    }
    public async Task UpdateProfileAsync(UpdateProfileRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var profile = await _userProfileRepository.GetByUserIdAsync(userId.Value, cancellationToken);
        if (profile is null)
            throw new NotFoundException("Profile not found.");

        profile.FullName = request.FullName?.Trim();
        profile.Bio = request.Bio?.Trim();
        profile.GithubUrl = request.GithubUrl?.Trim();
        profile.UpdatedAt = _dateTimeService.UtcNow;

        _userProfileRepository.Update(profile);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<UpdateAvatarResponseDto> UpdateAvatarAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User is not authenticated.");

        if (file is null || file.Length == 0)
            throw new BadRequestException("Avatar file is required.");

        var profile = await _userProfileRepository.GetByUserIdAsync(userId.Value, cancellationToken);
        if (profile is null)
            throw new NotFoundException("Profile not found.");

        if (!string.IsNullOrWhiteSpace(profile.AvatarPublicId))
        {
            await _cloudinaryService.DeleteImageAsync(profile.AvatarPublicId, cancellationToken);
        }

        await using var stream = file.OpenReadStream();
        var uploadResult = await _cloudinaryService.UploadImageAsync(stream, file.FileName, cancellationToken);

        profile.AvatarUrl = uploadResult.Url;
        profile.AvatarPublicId = uploadResult.PublicId;
        profile.UpdatedAt = _dateTimeService.UtcNow;

        _userProfileRepository.Update(profile);
        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateAvatarResponseDto
        {
            AvatarUrl = profile.AvatarUrl
        };
    }

    public async Task RemoveAvatarAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var profile = await _userProfileRepository.GetByUserIdAsync(userId.Value, cancellationToken);
        if (profile is null)
            throw new NotFoundException("Profile not found.");

        if (!string.IsNullOrWhiteSpace(profile.AvatarPublicId))
        {
            await _cloudinaryService.DeleteImageAsync(profile.AvatarPublicId, cancellationToken);
        }

        profile.AvatarUrl = null;
        profile.AvatarPublicId = null;
        profile.UpdatedAt = _dateTimeService.UtcNow;

        _userProfileRepository.Update(profile);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<UserProfileDto> BuildUserProfileDtoAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            throw new NotFoundException("User not found.");

        var profile = user.Profile ?? await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        var followerCount = await _userFollowRepository.CountFollowersAsync(userId, cancellationToken);

        return new UserProfileDto
        {
            UserId = user.Id,
            Email = user.Email,
            Username = user.Username,
            FullName = profile?.FullName,
            AvatarUrl = profile?.AvatarUrl,
            Bio = profile?.Bio,
            GithubUrl = profile?.GithubUrl,
            FollowerCount = followerCount
        };
    }
}