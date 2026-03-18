using DevOpsAiHub.Application.Common.Interfaces.Auth;
using Microsoft.Extensions.Caching.Memory;

namespace DevOpsAiHub.Infrastructure.Identity;

public class OtpService : IOtpService
{
    private readonly IMemoryCache _memoryCache;
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);

    public OtpService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task StoreRegisterOtpAsync(
        string email,
        string username,
        string passwordHash,
        string otp,
        CancellationToken cancellationToken = default)
    {
        var key = GetRegisterKey(email);

        var payload = new RegisterOtpCacheModel
        {
            Email = email,
            Username = username,
            PasswordHash = passwordHash,
            Otp = otp
        };

        _memoryCache.Set(key, payload, OtpLifetime);

        return Task.CompletedTask;
    }

    public Task<(bool Success, string Message)> VerifyRegisterOtpAsync(
        string email,
        string otp,
        CancellationToken cancellationToken = default)
    {
        var key = GetRegisterKey(email);

        if (!_memoryCache.TryGetValue<RegisterOtpCacheModel>(key, out var payload) || payload is null)
            return Task.FromResult((false, "OTP expired or not found."));

        if (payload.Otp != otp)
            return Task.FromResult((false, "Invalid OTP."));

        return Task.FromResult((true, "OTP verified successfully."));
    }

    public Task<(string Email, string Username, string PasswordHash)?> GetPendingRegisterAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var key = GetRegisterKey(email);

        if (!_memoryCache.TryGetValue<RegisterOtpCacheModel>(key, out var payload) || payload is null)
            return Task.FromResult<(string, string, string)?>(null);

        return Task.FromResult<(string, string, string)?>((payload.Email, payload.Username, payload.PasswordHash));
    }

    public Task RemoveRegisterOtpAsync(string email, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(GetRegisterKey(email));
        return Task.CompletedTask;
    }

    public Task StoreForgotPasswordOtpAsync(
        string email,
        string otp,
        CancellationToken cancellationToken = default)
    {
        var key = GetForgotPasswordKey(email);

        var payload = new ForgotPasswordOtpCacheModel
        {
            Email = email,
            Otp = otp
        };

        _memoryCache.Set(key, payload, OtpLifetime);

        return Task.CompletedTask;
    }

    public Task<(bool Success, string Message)> VerifyForgotPasswordOtpAsync(
        string email,
        string otp,
        CancellationToken cancellationToken = default)
    {
        var key = GetForgotPasswordKey(email);

        if (!_memoryCache.TryGetValue<ForgotPasswordOtpCacheModel>(key, out var payload) || payload is null)
            return Task.FromResult((false, "OTP expired or not found."));

        if (payload.Otp != otp)
            return Task.FromResult((false, "Invalid OTP."));

        return Task.FromResult((true, "OTP verified successfully."));
    }

    public Task RemoveForgotPasswordOtpAsync(string email, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(GetForgotPasswordKey(email));
        return Task.CompletedTask;
    }

    private static string GetRegisterKey(string email) => $"register_otp:{email}";
    private static string GetForgotPasswordKey(string email) => $"forgot_password_otp:{email}";

    private sealed class RegisterOtpCacheModel
    {
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Otp { get; set; } = null!;
    }

    private sealed class ForgotPasswordOtpCacheModel
    {
        public string Email { get; set; } = null!;
        public string Otp { get; set; } = null!;
    }
}