using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Validators;

namespace Rido.BFLite.Core.Hosting;

public static class WebApiSecurity
{
    private static IList<string> validTokenIssuers = ["https://api.botframework.com"];
    public static void AddBotFrameworkAuthentication(this IServiceCollection services, string tokenValidationSectionName = "AzureAd")
    {
        IConfiguration configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        string? tenantId = configuration[$"{tokenValidationSectionName}:TenantId"];
        string? clientId = configuration[$"{tokenValidationSectionName}:ClientId"];
        string? secret = configuration[$"{tokenValidationSectionName}:ClientCredentials:0:ClientSecret"];
        string? agentScope = configuration[$"{tokenValidationSectionName}:AgentScope"];

        string dir = string.IsNullOrEmpty(tenantId) ? "botframework.com" : tenantId;
        validTokenIssuers.Add($"https://login.microsoftonline.com/{dir}/v2.0");
        services
            .AddTokenAcquisition(true)
            .AddInMemoryTokenCaches()
            .AddAuthentication()
            .AddMicrosoftIdentityWebApi(configuration.GetSection(tokenValidationSectionName), JwtBearerDefaults.AuthenticationScheme, true);

        if (!string.IsNullOrEmpty(agentScope))
        {
            services.AddAgentIdentities();
        }

        ConfigureIncomingTokenValidation(services, tokenValidationSectionName, configuration, tenantId, agentScope);

        services.Configure<MicrosoftIdentityApplicationOptions>(ops =>
        {
            ops.Instance = "https://login.microsoftonline.com/";
            ops.TenantId = tenantId;
            ops.ClientId = clientId;
            ops.ClientCredentials = [
                new CredentialDescription()
        {
            //SourceType = CredentialSource.SignedAssertionFromManagedIdentity,
            //ManagedIdentityClientId = miClientId
            SourceType = CredentialSource.ClientSecret,
            ClientSecret = secret
        }
            ];
        });

    }

    private static void ConfigureIncomingTokenValidation(IServiceCollection services, string tokenValidationSectionName, IConfiguration configuration, string? tenantId, string? agentScope)
    {
        services.Configure<JwtBearerOptions>("Bearer", options =>
        {
            options.SaveToken = true;
            string cid = configuration[$"{tokenValidationSectionName}:ClientId"]!;


            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuers = validTokenIssuers,
                ValidAudiences = [configuration[$"{tokenValidationSectionName}:ClientId"], "https://api.botframework.com"],
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                RequireSignedTokens = true,
                ClockSkew = TimeSpan.FromMinutes(5),
            };

            string oidcAuthority = agentScope is null || agentScope!.Equals("https://api.botframework.com/.default", StringComparison.OrdinalIgnoreCase)
                ? "https://login.botframework.com/v1/.well-known/openid-configuration"
                : $"https://login.microsoftonline.com/{tenantId ?? "botframework.com"}/v2.0/.well-known/openid-configuration";

            options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                oidcAuthority,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever
                {
                    RequireHttps = options.RequireHttpsMetadata
                });

            options.TokenValidationParameters.EnableAadSigningKeyIssuerValidation();
        });

#pragma warning disable ASP0025 // Use AddAuthorizationBuilder
        services.AddAuthorization(options =>
        {
            options.AddPolicy("Bot", policy =>
            {
                //policy.RequireAssertion(_ => true);
                policy.RequireClaim("aud");
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("aud", [configuration[$"{tokenValidationSectionName}:ClientId"]!]);
            });
        });
#pragma warning restore ASP0025 // Use AddAuthorizationBuilder
    }
}
