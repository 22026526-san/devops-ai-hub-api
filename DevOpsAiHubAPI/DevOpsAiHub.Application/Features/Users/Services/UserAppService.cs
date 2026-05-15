using DevOpsAiHub.Application.Common.Exceptions;
using DevOpsAiHub.Application.Common.Interfaces.Auth;
using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Common.Models;
using DevOpsAiHub.Application.Features.Users.DTOs;
using DevOpsAiHub.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


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
    private readonly IEmailService _emailService;
    private readonly IOtpService _otpService;
    private readonly IPasswordHasherService _passwordHasherService;

    public UserAppService(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IUserProfileRepository userProfileRepository,
        IUserFollowRepository userFollowRepository,
        ICloudinaryService cloudinaryService,
        IDateTimeService dateTimeService,
        IApplicationDbContext context,
        IEmailService emailService,
        IPasswordHasherService passwordHasherService,
        IOtpService otpService)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _userProfileRepository = userProfileRepository;
        _userFollowRepository = userFollowRepository;
        _cloudinaryService = cloudinaryService;
        _dateTimeService = dateTimeService;
        _context = context;
        _emailService = emailService;
        _otpService = otpService;
        _passwordHasherService = passwordHasherService;
    }

    public async Task UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            throw new NotFoundException("User not found.");

        var role = request.Role?.Trim();

        if (string.IsNullOrWhiteSpace(role))
            throw new BadRequestException("Role is required.");

        if (role != UserRole.User && role != UserRole.Admin)
            throw new BadRequestException("Invalid role.");

        user.Role = role;
        user.UpdatedAt = _dateTimeService.UtcNow;

        _userRepository.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateUserStatusAsync(Guid userId, UpdateUserStatusRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            throw new NotFoundException("User not found.");

        var status = request.Status?.Trim();

        if (string.IsNullOrWhiteSpace(status))
            throw new BadRequestException("Status is required.");

        if (status != UserStatus.Active && status != UserStatus.Locked)
            throw new BadRequestException("Invalid status.");

        user.Status = status;
        user.UpdatedAt = _dateTimeService.UtcNow;

        _userRepository.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
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

    public async Task<PagedResult<UserProfileDto>> GetAllProfilesAsync(GetUserQueryDto request,CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var userIds = users.Select(x => x.Id).ToList();
        var followerMap = await _userFollowRepository.CountFollowersByUserIdsAsync(userIds, cancellationToken);
        var followingMap = await _userFollowRepository.CountFollowingByUserIdsAsync(userIds, cancellationToken);

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim();
            users = users.Where(x => x.Profile?.FullName?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
        }

        var totalItems = users.Count;

        var pagedUsers = users
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var items = pagedUsers.Select(user =>
        {
            var profile = user.Profile;
            followerMap.TryGetValue(user.Id, out var followerCount);
            followingMap.TryGetValue(user.Id, out var followingCount);

            return new UserProfileDto
            {
                UserId = user.Id,
                Email = user.Email,
                Username = user.Username,
                FullName = profile?.FullName,
                JobTitle = profile?.JobTitle,
                AvatarUrl = profile?.AvatarUrl,
                Bio = profile?.Bio,
                GithubUrl = profile?.GithubUrl,
                FollowerCount = followerCount,
                FollowingCount = followingCount,
                Status = user.Status,
                CreatedAt = profile?.CreatedAt
            };
        }).ToList();
        return new PagedResult<UserProfileDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            HasNextPage = page * pageSize < totalItems
        };
    }

    public async Task<List<UserProfileDto>> GetSuggestedProfilesAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;

        var excludedUserIds = new HashSet<Guid>();

        var users = await _userRepository.GetAllAsync(cancellationToken);


        if (currentUserId.HasValue)
        {
            var followingUserIds = await _userFollowRepository.GetFollowingUserIdsAsync(currentUserId.Value, cancellationToken);

            excludedUserIds = new HashSet<Guid>(followingUserIds)
            {
                currentUserId.Value 
            };
        }

        var candidateUsers = users
            .Where(x => !excludedUserIds.Contains(x.Id))
            .ToList();

        var candidateUserIds = candidateUsers.Select(x => x.Id).ToList();
        var followerMap = await _userFollowRepository.CountFollowersByUserIdsAsync(candidateUserIds, cancellationToken);
        var followingMap = await _userFollowRepository.CountFollowingByUserIdsAsync(candidateUserIds, cancellationToken);

        return candidateUsers
            .Select(user =>
            {
                var profile = user.Profile;
                followerMap.TryGetValue(user.Id, out var followerCount);
                followingMap.TryGetValue(user.Id, out var followingCount);

                return new UserProfileDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    FullName = profile?.FullName,
                    JobTitle = profile?.JobTitle,
                    AvatarUrl = profile?.AvatarUrl,
                    Bio = profile?.Bio,
                    GithubUrl = profile?.GithubUrl,
                    Status = user.Status,
                    FollowerCount = followerCount,
                    FollowingCount = followingCount
                };
            })
            .OrderByDescending(x => x.FollowerCount)
            .ThenBy(x => x.Username)
            .Take(20)
            .ToList();
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
        profile.JobTitle = request.JobTitle?.Trim();
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

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
        {
            throw new BadRequestException("Định dạng file không được hỗ trợ.");
        }

        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedMimeTypes.Contains(file.ContentType.ToLower()))
        {
            throw new BadRequestException("File không phải là hình ảnh hợp lệ.");
        }

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
        var currentUserId = _currentUserService.UserId;
        if (user is null)
            throw new NotFoundException("User not found.");

        var profile = user.Profile ?? await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        var followerCount = await _userFollowRepository.CountFollowersAsync(userId, cancellationToken);
        var followingCount = await _userFollowRepository.CountFollowingAsync(userId, cancellationToken);
        var isFollowing = false;

        if (currentUserId != null && currentUserId != userId)
        {
            isFollowing = await _context.UserFollows
                .AnyAsync(x => x.FollowerId == currentUserId.Value && x.FollowingId == userId, cancellationToken);
        }

        return new UserProfileDto
        {
            UserId = user.Id,
            Role = user.Role,
            Email = user.Email,
            Username = user.Username,
            FullName = profile?.FullName,
            JobTitle = profile?.JobTitle,
            AvatarUrl = profile?.AvatarUrl,
            Bio = profile?.Bio,
            GithubUrl = profile?.GithubUrl,
            Status = user.Status,
            FollowerCount = followerCount,
            FollowingCount = followingCount,
            IsFollowing = isFollowing,
            CreatedAt = profile?.CreatedAt
        };
    }
    public async Task VerifyUpdateUserOtpAsync(VerifyUpdateUserOtpDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var user = await _userRepository.GetByIdAsync(currentUserId.Value, cancellationToken);
        if (user is null)
            throw new NotFoundException("User not found.");

        var oldEmail = user.Email;

        if (string.IsNullOrWhiteSpace(request.Otp))
            throw new BadRequestException("OTP is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BadRequestException("Email is required.");

        if (string.IsNullOrWhiteSpace(request.UserName))
            throw new BadRequestException("UserName is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new BadRequestException("Password is required.");

        var verifyOtpResult = await _otpService.VerifyUpdateUserOtpAsync(oldEmail, request.Otp.Trim(), cancellationToken);
        if (!verifyOtpResult.Success)
            throw new BadRequestException(verifyOtpResult.Message);

        var newEmail = request.Email.Trim();
        var newUserName = request.UserName.Trim();

        if (!string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
        {
            var emailExists = await _userRepository.ExistsByEmailAsync(newEmail, cancellationToken);
            if (emailExists)
                throw new BadRequestException("Email already exists.");
        }

        if (!string.Equals(user.Username, newUserName, StringComparison.OrdinalIgnoreCase))
        {
            var usernameExists = await _userRepository.ExistsByUsernameAsync(newUserName, cancellationToken);
            if (usernameExists)
                throw new BadRequestException("UserName already exists.");
        }

        user.Email = newEmail;
        user.Username = newUserName;
        user.PasswordHash = _passwordHasherService.HashPassword(request.Password.Trim());
        user.UpdatedAt = _dateTimeService.UtcNow;

        _userRepository.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        await _otpService.RemoveUpdateUserOtpAsync(oldEmail, cancellationToken);
    }
    public async Task UpdateUserOtpRequestAsync(UpdateUserOtpRequestDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var user = await _userRepository.GetByIdAsync(currentUserId.Value, cancellationToken);
        if (user is null)
            throw new NotFoundException("User not found.");

        if (string.IsNullOrWhiteSpace(request.OldEmail))
            throw new BadRequestException("Old email is required.");

        if (!string.Equals(user.Email, request.OldEmail.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException("Old email does not match current email.");

        var otp = new Random().Next(100000, 999999).ToString();

        await _otpService.StoreUpdateUserOtpAsync(user.Email, otp, cancellationToken);

        await _emailService.SendOtpAsync(user.Email,otp, "OTP for update Users", cancellationToken);
    }
}