using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Validators;

namespace Rido.BFLite.Core.Hosting;

public static class WebApiSecurity
{
    private static readonly string[] validTokenIssuers = ["https://api.botframework.com"];
    public static void AddBotFrameworkAuthentication(this IServiceCollection services, string tokenValidationSectionName = "AzureAd")
    {
        IConfiguration configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        services
            .AddAuthentication()
            .AddMicrosoftIdentityWebApi(configuration.GetSection(tokenValidationSectionName), JwtBearerDefaults.AuthenticationScheme, true)
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddInMemoryTokenCaches();

        services.Configure<JwtBearerOptions>("Bearer", options =>
        {
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuers = validTokenIssuers,
                ValidAudience = configuration[$"{tokenValidationSectionName}:ClientId"],
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                RequireSignedTokens = true,
                ClockSkew = TimeSpan.FromMinutes(5),
            };

            options.TokenValidationParameters.EnableAadSigningKeyIssuerValidation();
            options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                "https://login.botframework.com/v1/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever
                {
                    RequireHttps = options.RequireHttpsMetadata
                });
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
