using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
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
        string dir = string.IsNullOrEmpty(tenantId) ? "botframework.com" : tenantId;
        validTokenIssuers.Add($"https://login.microsoftonline.com/{dir}/v2.0");
        services
            .AddAuthentication()
            .AddMicrosoftIdentityWebApi(configuration.GetSection(tokenValidationSectionName), JwtBearerDefaults.AuthenticationScheme, true)
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddInMemoryTokenCaches();

        services.AddScoped<IAuthorizationHeaderProvider, AgenticCredentialsProvider>();

        services.Configure<JwtBearerOptions>("Bearer", options =>
        {
            options.SaveToken = true;
            string cid = configuration[$"{tokenValidationSectionName}:ClientId"]!;
            string? abp = configuration[$"{tokenValidationSectionName}:ABP"];
            string validAudience = cid;
            if (!string.IsNullOrEmpty(abp))
            {
                validAudience = abp;
            }

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuers = validTokenIssuers,
                //ValidAudiences = [cid, abp],
                ValidAudience = validAudience,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                RequireSignedTokens = true,
                ClockSkew = TimeSpan.FromMinutes(5),
            };

            string oidcAuthority = string.IsNullOrEmpty(abp)
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
