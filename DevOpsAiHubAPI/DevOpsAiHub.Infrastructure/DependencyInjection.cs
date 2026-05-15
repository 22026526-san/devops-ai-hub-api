using DevOpsAiHub.Application.Common.Interfaces.Auth;
using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.AI.UseCase;
using DevOpsAiHub.Infrastructure.Identity;
using DevOpsAiHub.Infrastructure.Options;
using DevOpsAiHub.Infrastructure.Persistence;
using DevOpsAiHub.Infrastructure.Persistence.Repositories;
using DevOpsAiHub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace DevOpsAiHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        var ollamaBase = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        var chatModel = configuration["Ollama:ChatModel"] ?? "qwen3.5:9b";
        var embedModel = configuration["Ollama:EmbedModel"] ?? "bge-m3:567m";

        services.AddHttpClient("Qdrant");

        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        services.Configure<EmailOptions>(
            configuration.GetSection(EmailOptions.SectionName));

        services.Configure<CloudinaryOptions>(
            configuration.GetSection(CloudinaryOptions.SectionName));

        services.Configure<QdrantOptions>(configuration.GetSection("Qdrant"));
        services.Configure<OllamaOptions>(configuration.GetSection("Ollama"));
        services.Configure<RagOptions>(configuration.GetSection("Rag"));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(
                configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))));


        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddSingleton<IChatClient>(_ =>
            new OllamaChatClient(new Uri(ollamaBase), chatModel));

        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(_ =>
            new OllamaEmbeddingGenerator(new Uri(ollamaBase), embedModel));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IUserFollowRepository, UserFollowRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IPipelineRepository, PipelineRepository>();
        services.AddScoped<ISlugService, SlugService>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ILikeRepository, LikeRepository>();
        services.AddScoped<IBookmarkRepository, BookmarkRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IPostTagRepository, PostTagRepository>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IOtpService, OtpService>();

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddScoped<IDateTimeService, DateTimeService>();

        services.AddScoped<IAiConversationRepository, AiConversationRepository>();
        services.AddScoped<IAiMessageRepository, AiMessageRepository>();

        services.AddSingleton<IEmbeddingService, EmbeddingService>();
        services.AddScoped<ILlmService, LlmService>();
        services.AddSingleton<IRerankService, OnnxRerankService>();

        services.AddScoped<IVectorCollectionService, QdrantVectorCollectionService>();
        services.AddScoped<IRagSearchService, RagSearchService>();
        services.AddSingleton<ITextChunkerService, TextChunkerService>();

        services.AddScoped<IAiConversationRepository, AiConversationRepository>();
        services.AddScoped<IAiMessageRepository, AiMessageRepository>();
        services.AddScoped<AiChatUseCase>();
        services.AddScoped<IngestDocumentUseCase>();


        services.AddHttpContextAccessor();
        services.AddMemoryCache();

        return services;
    }
}