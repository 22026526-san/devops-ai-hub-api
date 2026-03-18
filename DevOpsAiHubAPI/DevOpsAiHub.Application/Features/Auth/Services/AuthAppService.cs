using DevOpsAiHub.Application.Common.Exceptions;
using DevOpsAiHub.Application.Common.Interfaces.Auth;
using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.Auth.DTOs;
using DevOpsAiHub.Domain.Entities.Users;
using DevOpsAiHub.Domain.Enums;

namespace DevOpsAiHub.Application.Features.Auth.Services;

public class AuthAppService : IAuthAppService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IApplicationDbContext _context;

    public AuthAppService(
        IUserRepository userRepository,
        IUserProfileRepository userProfileRepository,
        IPasswordHasherService passwordHasherService,
        IJwtTokenService jwtTokenService,
        IOtpService otpService,
        IEmailService emailService,
        IDateTimeService dateTimeService,
        IApplicationDbContext context)
    {
        _userRepository = userRepository;
        _userProfileRepository = userProfileRepository;
        _passwordHasherService = passwordHasherService;
        _jwtTokenService = jwtTokenService;
        _otpService = otpService;
        _emailService = emailService;
        _dateTimeService = dateTimeService;
        _context = context;
    }

    public async Task RequestRegisterOtpAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var username = request.Username.Trim();

        if (string.IsNullOrWhiteSpace(email))
            throw new BadRequestException("Email is required.");

        if (string.IsNullOrWhiteSpace(username))
            throw new BadRequestException("Username is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new BadRequestException("Password is required.");

        var emailExists = await _userRepository.ExistsByEmailAsync(email, cancellationToken);
        if (emailExists)
            throw new BadRequestException("Email already exists.");

        var usernameExists = await _userRepository.ExistsByUsernameAsync(username, cancellationToken);
        if (usernameExists)
            throw new BadRequestException("Username already exists.");

        var passwordHash = _passwordHasherService.HashPassword(request.Password);
        var otp = GenerateOtp();

        await _otpService.StoreRegisterOtpAsync(email, username, passwordHash, otp, cancellationToken);
        await _emailService.SendOtpAsync(email, otp, "OTP for registration", cancellationToken);
    }

    public async Task<AuthResponseDto> VerifyRegisterOtpAsync(VerifyRegisterOtpRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var verifyResult = await _otpService.VerifyRegisterOtpAsync(email, request.Otp, cancellationToken);
        if (!verifyResult.Success)
            throw new BadRequestException(verifyResult.Message);

        var pendingRegister = await _otpService.GetPendingRegisterAsync(email, cancellationToken);
        if (pendingRegister is null)
            throw new BadRequestException("Register session expired.");

        var emailExists = await _userRepository.ExistsByEmailAsync(email, cancellationToken);
        if (emailExists)
            throw new BadRequestException("Email already exists.");

        var usernameExists = await _userRepository.ExistsByUsernameAsync(pendingRegister.Value.Username, cancellationToken);
        if (usernameExists)
            throw new BadRequestException("Username already exists.");

        var now = _dateTimeService.UtcNow;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = pendingRegister.Value.Email,
            Username = pendingRegister.Value.Username,
            PasswordHash = pendingRegister.Value.PasswordHash,
            Role = UserRole.User,
            Status = UserStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };

        var profile = new UserProfile
        {
            UserId = user.Id,
            FullName = pendingRegister.Value.Username,
            AvatarUrl = null,
            AvatarPublicId = null,
            Bio = null,
            GithubUrl = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userProfileRepository.AddAsync(profile, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _otpService.RemoveRegisterOtpAsync(email, cancellationToken);

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Username = user.Username,
            Role = user.Role
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.EmailOrUsername))
            throw new BadRequestException("Email or username is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new BadRequestException("Password is required.");

        var input = request.EmailOrUsername.Trim();

        User? user;
        if (input.Contains("@"))
        {
            user = await _userRepository.GetByEmailAsync(input.ToLowerInvariant(), cancellationToken);
        }
        else
        {
            user = await _userRepository.GetByUsernameAsync(input, cancellationToken);
        }

        if (user is null)
            throw new UnauthorizedException("Invalid credentials.");

        if (user.Status != UserStatus.Active)
            throw new UnauthorizedException("Account is not active.");

        var isPasswordValid = _passwordHasherService.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
            throw new UnauthorizedException("Invalid credentials.");

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Username = user.Username,
            Role = user.Role
        };
    }

    public async Task RequestForgotPasswordOtpAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email))
            throw new BadRequestException("Email is required.");

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
            throw new NotFoundException("Email does not exist.");

        var otp = GenerateOtp();

        await _otpService.StoreForgotPasswordOtpAsync(email, otp, cancellationToken);
        await _emailService.SendOtpAsync(email, otp, "OTP for reset password", cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email))
            throw new BadRequestException("Email is required.");

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            throw new BadRequestException("New password is required.");

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
            throw new NotFoundException("Email does not exist.");

        var verifyResult = await _otpService.VerifyForgotPasswordOtpAsync(email, request.Otp, cancellationToken);
        if (!verifyResult.Success)
            throw new BadRequestException(verifyResult.Message);

        user.PasswordHash = _passwordHasherService.HashPassword(request.NewPassword);
        user.UpdatedAt = _dateTimeService.UtcNow;

        _userRepository.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        await _otpService.RemoveForgotPasswordOtpAsync(email, cancellationToken);
    }

    private static string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}