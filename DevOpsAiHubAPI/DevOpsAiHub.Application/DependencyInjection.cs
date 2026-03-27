using DevOpsAiHub.Application.Features.App.Tags.Services;
using DevOpsAiHub.Application.Features.Auth.Services;
using DevOpsAiHub.Application.Features.Users.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DevOpsAiHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthAppService, AuthAppService>();
        services.AddScoped<IUserAppService, UserAppService>();
        services.AddScoped<ITagAppService, TagAppService>();

        return services;
    }
}