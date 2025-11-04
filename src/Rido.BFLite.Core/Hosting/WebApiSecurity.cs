using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

    private static readonly IList<string> validTokenIssuers = ["https://api.botframework.com"];
    public static void AddBotFrameworkAuthentication(this IServiceCollection services, string aadConfigSectionName = "AzureAd")
    {
        ConversationClient ConversationClientFactory(IServiceProvider provider) => new(
            provider.GetService<IConfiguration>()!,
            provider.GetService<IHttpClientFactory>()!,
            provider.GetService<ILogger<ConversationClient>>()!,
            provider.GetService<IAuthorizationHeaderProvider>()!,
            aadConfigSectionName
            );

        services.AddScoped<ConversationClient>(ConversationClientFactory);
        services.AddScoped<UserTokenClient>();

        IConfiguration configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        string? tenantId = configuration[$"{aadConfigSectionName}:TenantId"];
        string? clientId = configuration[$"{aadConfigSectionName}:ClientId"];
        string? secret = configuration[$"{aadConfigSectionName}:ClientCredentials:0:ClientSecret"];
        string? agentScope = configuration[$"{aadConfigSectionName}:AgentScope"];

        string dir = string.IsNullOrEmpty(tenantId) ? "botframework.com" : tenantId;
        validTokenIssuers.Add($"https://login.microsoftonline.com/{dir}/v2.0");
        services
            .AddTokenAcquisition(true)
            .AddInMemoryTokenCaches()
            .AddAuthentication()
            .AddMicrosoftIdentityWebApi(configuration.GetSection(aadConfigSectionName), JwtBearerDefaults.AuthenticationScheme, true);

        if (!string.IsNullOrEmpty(agentScope))
        {
            services.AddAgentIdentities();
        }

        ConfigureIncomingTokenValidation(services, aadConfigSectionName, configuration, tenantId, agentScope);

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

            //options.Events = new JwtBearerEvents
            //{
            //    OnAuthenticationFailed = context =>
            //    {
            //        var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtBearer");
            //        logger.LogError(context.Exception, "Authentication failed.");
            //        return Task.CompletedTask;
            //    },
            //    OnTokenValidated = context =>
            //    {
            //        // Additional custom validation can be added here if needed
            //        return Task.CompletedTask;
            //    },
            //    OnForbidden = context =>
            //    {
            //        var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtBearer");
            //        logger.LogWarning("Forbidden: {Message}", context.Result?.ToString());
            //        return Task.CompletedTask;
            //    },
            //    OnChallenge = context =>
            //    {
            //        var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtBearer");
            //        logger.LogWarning("Challenge: {Message}", context.ErrorDescription);
            //        return Task.CompletedTask;
            //    }
            //};

        });




        services.AddAuthorization(options =>
        {
            options.AddPolicy("Bot", policy =>
            {
                policy.RequireAssertion(_ => true);
                //policy.RequireClaim("aud");
                //policy.RequireAuthenticatedUser();
                //policy.RequireClaim("aud", [configuration[$"{tokenValidationSectionName}:ClientId"]!]);
            });

            options.AddPolicy("Agent", policy =>
            {
                policy.RequireAssertion(_ => true);
                //policy.RequireClaim("aud");
                //policy.RequireAuthenticatedUser();
                //policy.RequireClaim("aud", [configuration[$"{tokenValidationSectionName}:ClientId"]!]);
            });
        });
    }
}
