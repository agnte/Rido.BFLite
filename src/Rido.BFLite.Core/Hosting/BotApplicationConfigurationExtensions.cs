using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;

namespace Rido.BFLite.Core.Hosting;

public static class BotApplicationConfigurationExtensions
{
    /// <summary>
    /// Named HttpClient for Bot Framework conversation operations.
    /// </summary>
    public const string ConversationHttpClientName = "BotFrameworkConversation";

    /// <summary>
    /// Named HttpClient for Bot Framework user token operations.
    /// </summary>
    public const string UserTokenHttpClientName = "BotFrameworkUserToken";

    public static IServiceCollection AddBotApplication<TApp>(this IServiceCollection services) where TApp : BotApplication, new()
    {
        services.AddBotApplicationClients();
        services.AddSingleton<TApp>();
        return services;
    }

    public static IServiceCollection AddBotApplication<TApp>(this IServiceCollection services, TApp app) where TApp : BotApplication, new()
    {
        services.AddBotApplicationClients();
        services.AddSingleton(app);
        return services;
    }

    private static IServiceCollection AddBotApplicationClients(this IServiceCollection services, string aadConfigSectionName = "AzureAd")
    {
        IConfiguration configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        services
            .AddHttpClient()
            .AddTokenAcquisition(false)
            .AddInMemoryTokenCaches()
            .AddAgentIdentities();
            
        services.Configure<MicrosoftIdentityApplicationOptions>(aadConfigSectionName, configuration.GetSection(aadConfigSectionName));

        services.AddScoped<AgenticAuthorizationHeaderProviderService>();

        // Get the agent scope from configuration for conversation client
        string agentScope = configuration[$"{aadConfigSectionName}:AgentScope"] ?? "https://api.botframework.com/.default";

        // Configure HttpClient for ConversationClient with the authentication handler
        // Uses the agent scope from configuration
        services.AddHttpClient(ConversationHttpClientName)
            .AddHttpMessageHandler(sp => new BotAuthenticationHandler(
                sp.GetRequiredService<AgentAuthorizationHeaderProviderService>(),
                agentScope,
                aadConfigSectionName));

        // Configure HttpClient for UserTokenClient with the authentication handler
        // Uses the fixed bot framework scope for token service API calls
        services.AddHttpClient(UserTokenHttpClientName)
            .AddHttpMessageHandler(sp => new BotAuthenticationHandler(
                sp.GetRequiredService<AgentAuthorizationHeaderProviderService>(),
                "https://api.botframework.com/.default",
                aadConfigSectionName));

        static ConversationClient ConversationClientFactory(IServiceProvider provider, object serviceKey) => new(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(ConversationHttpClientName),
            provider.GetService<ILogger<ConversationClient>>()!
            );

        services.AddKeyedScoped(aadConfigSectionName, ConversationClientFactory);

        services.AddScoped(sp => new UserTokenClient(
            sp.GetRequiredService<ILogger<UserTokenClient>>(),
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(UserTokenHttpClientName)));

        return services;
    }
}
