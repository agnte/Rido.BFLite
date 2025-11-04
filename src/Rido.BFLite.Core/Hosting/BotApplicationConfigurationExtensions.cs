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

    public static IServiceCollection AddBotApplicationClients(this IServiceCollection services, string aadConfigSectionName = "AzureAd")
    {
        IConfiguration configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        services
            .AddHttpClient()
            .AddTokenAcquisition(true)
            .AddInMemoryTokenCaches()
            .AddAgentIdentities();
            
        services.Configure<MicrosoftIdentityApplicationOptions>("Bearer", configuration.GetSection(aadConfigSectionName));

        ConversationClient ConversationClientFactory(IServiceProvider provider) => new(
            provider.GetService<IConfiguration>()!,
            provider.GetService<IHttpClientFactory>()!,
            provider.GetService<ILogger<ConversationClient>>()!,
            provider.GetService<IAuthorizationHeaderProvider>()!,
            aadConfigSectionName
            );

        services.AddScoped(ConversationClientFactory);
        services.AddScoped<UserTokenClient>();
        return services;
    }
}
