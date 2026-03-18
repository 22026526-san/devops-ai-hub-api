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
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IApplicationDbContext _context;

    public UserAppService(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IUserProfileRepository userProfileRepository,
        ICloudinaryService cloudinaryService,
        IDateTimeService dateTimeService,
        IApplicationDbContext context)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _userProfileRepository = userProfileRepository;
        _cloudinaryService = cloudinaryService;
        _dateTimeService = dateTimeService;
        _context = context;
    }

    public async Task<UserProfileDto> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user is null)
            throw new NotFoundException("User not found.");

        var profile = user.Profile ?? await _userProfileRepository.GetByUserIdAsync(userId.Value, cancellationToken);

        return new UserProfileDto
        {
            UserId = user.Id,
            Email = user.Email,
            Username = user.Username,
            FullName = profile?.FullName,
            AvatarUrl = profile?.AvatarUrl,
            Bio = profile?.Bio,
            GithubUrl = profile?.GithubUrl
        };
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
}