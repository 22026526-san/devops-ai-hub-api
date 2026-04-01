using DevOpsAiHub.Application.Common.Exceptions;
using DevOpsAiHub.Application.Common.Interfaces.Auth;
using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.App.Follows.DTOs;
using DevOpsAiHub.Domain.Entities.Users;
using DevOpsAiHub.Domain.Enums;

namespace DevOpsAiHub.Application.Features.App.Follows.Services;

public class FollowAppService : IFollowAppService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IUserFollowRepository _userFollowRepository;
    private readonly IDateTimeService _dateTimeService;
    private readonly IApplicationDbContext _context;

    public FollowAppService(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IUserFollowRepository userFollowRepository,
        IDateTimeService dateTimeService,
        IApplicationDbContext context)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _userFollowRepository = userFollowRepository;
        _dateTimeService = dateTimeService;
        _context = context;
    }

    public async Task FollowUserAsync(Guid targetUserId, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        if (currentUserId.Value == targetUserId)
            throw new BadRequestException("You cannot follow yourself.");

        var targetUser = await _userRepository.GetByIdAsync(targetUserId, cancellationToken);
        if (targetUser is null)
            throw new NotFoundException("Target user not found.");

        if (targetUser.Status != UserStatus.Active)
            throw new BadRequestException("Target user is not active.");

        var alreadyFollowed = await _userFollowRepository.ExistsAsync(currentUserId.Value, targetUserId, cancellationToken);
        if (alreadyFollowed)
            throw new BadRequestException("You already follow this user.");

        var userFollow = new UserFollow
        {
            Id = Guid.NewGuid(),
            FollowerId = currentUserId.Value,
            FollowingId = targetUserId,
            CreatedAt = _dateTimeService.UtcNow
        };

        await _userFollowRepository.AddAsync(userFollow, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UnfollowUserAsync(Guid targetUserId, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var userFollow = await _userFollowRepository.GetByFollowerAndFollowingAsync(currentUserId.Value, targetUserId, cancellationToken);
        if (userFollow is null)
            throw new NotFoundException("Follow relationship not found.");

        _userFollowRepository.Remove(userFollow);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<UserFollowDto>> GetFollowersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var targetUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (targetUser is null)
            throw new NotFoundException("User not found.");

        var followers = await _userFollowRepository.GetFollowersAsync(userId, cancellationToken);

        return followers.Select(x => new UserFollowDto
        {
            UserId = x.Follower.Id,
            Username = x.Follower.Username,
            Email = x.Follower.Email,
            FullName = x.Follower.Profile?.FullName,
            AvatarUrl = x.Follower.Profile?.AvatarUrl,
            FollowedAt = x.CreatedAt
        }).ToList();
    }

    public async Task<List<UserFollowDto>> GetFollowingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var targetUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (targetUser is null)
            throw new NotFoundException("User not found.");

        var following = await _userFollowRepository.GetFollowingAsync(userId, cancellationToken);

        return following.Select(x => new UserFollowDto
        {
            UserId = x.Following.Id,
            Username = x.Following.Username,
            Email = x.Following.Email,
            FullName = x.Following.Profile?.FullName,
            AvatarUrl = x.Following.Profile?.AvatarUrl,
            FollowedAt = x.CreatedAt
        }).ToList();
    }
}