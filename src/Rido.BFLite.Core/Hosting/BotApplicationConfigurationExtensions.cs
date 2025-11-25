using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;

namespace Rido.BFLite.Core.Hosting;

public static class BotApplicationConfigurationExtensions
{
    public static IServiceCollection AddBotApplication<TApp>(this IServiceCollection services) where TApp : BotApplication, new()
    {
        services.AddSingleton<TApp>();
        return services;
    }

    public static IServiceCollection AddBotApplication<TApp>(this IServiceCollection services, TApp app) where TApp : BotApplication, new()
    {
        services.AddSingleton(app);
        return services;
    }

    public static IServiceCollection AddBotApplicationClients(this IServiceCollection services, string aadConfigSectionName = "AzureAd")
    {
        IConfiguration configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        services
            .AddHttpClient()
            .AddTokenAcquisition(false)
            .AddInMemoryTokenCaches()
            .AddAgentIdentities();
            
        services.Configure<MicrosoftIdentityApplicationOptions>(aadConfigSectionName, configuration.GetSection(aadConfigSectionName));

        services.AddScoped<AgentAuthorizationHeaderProviderService>();

        static ConversationClient ConversationClientFactory(IServiceProvider provider, object serviceKey) => new(
            provider.GetService<IConfiguration>()!,
            provider.GetService<IHttpClientFactory>()!,
            provider.GetService<ILogger<ConversationClient>>()!,
            provider.GetService<AgentAuthorizationHeaderProviderService>()!,
            serviceKey.ToString()!
            );

        services.AddKeyedScoped(aadConfigSectionName, ConversationClientFactory);
        services.AddScoped<UserTokenClient>();
        return services;
    }
}
