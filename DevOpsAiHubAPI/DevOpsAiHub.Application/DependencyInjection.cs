using DevOpsAiHub.Application.Features.App.Bookmarks.Services;
using DevOpsAiHub.Application.Features.App.Comments.Services;
using DevOpsAiHub.Application.Features.App.Follows.Services;
using DevOpsAiHub.Application.Features.App.Likes.Services;
using DevOpsAiHub.Application.Features.App.Pipelines.Services;
using DevOpsAiHub.Application.Features.App.Posts.Services;
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
        services.AddScoped<IFollowAppService, FollowAppService>();
        services.AddScoped<IPostAppService, PostAppService>();
        services.AddScoped<IPipelineAppService, PipelineAppService>();
        services.AddScoped<ICommentAppService, CommentAppService>();
        services.AddScoped<ILikeAppService, LikeAppService>();
        services.AddScoped<IBookmarkAppService, BookmarkAppService>();

        return services;
    }
}