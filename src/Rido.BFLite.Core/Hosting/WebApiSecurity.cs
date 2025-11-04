using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Validators;

namespace Rido.BFLite.Core.Hosting;

public static class WebApiSecurity
{
    public static void AddBotFrameworkAuthentication(this IServiceCollection services, string aadConfigSectionName = "AzureAd")
    {
        IList<string> validTokenIssuers = ["https://api.botframework.com"];
        ILogger logger = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>().CreateLogger("WebApiSecurity");
        IConfiguration configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

        string? tenantId = configuration[$"{aadConfigSectionName}:TenantId"];
        string? agentScope = configuration[$"{aadConfigSectionName}:AgentScope"];
        
        string dir = string.IsNullOrEmpty(tenantId) ? "botframework.com" : tenantId;
        validTokenIssuers.Add($"https://login.microsoftonline.com/{dir}/v2.0");
        
        services
            .AddAuthentication()
            .AddMicrosoftIdentityWebApi(configuration.GetSection(aadConfigSectionName), JwtBearerDefaults.AuthenticationScheme, true);

        services.Configure<JwtBearerOptions>("Bearer", options =>
        {
            options.SaveToken = true;
            string cid = configuration[$"{aadConfigSectionName}:ClientId"]!;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuers = validTokenIssuers,
                ValidAudiences = [configuration[$"{aadConfigSectionName}:ClientId"], "https://api.botframework.com"],
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

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = context.Request.Headers.Authorization.FirstOrDefault();

                    Microsoft.IdentityModel.JsonWebTokens.JsonWebToken jwt = new (token!["Bearer ".Length..]);
                    var textClaims = string.Empty;
                    jwt.Claims.ToList().ForEach(c => textClaims += $"{c.Type}:{c.Value}\r\n");
                    logger.LogInformation("OnMessageReceived: {Token}", textClaims);
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    logger.LogError(context.Exception, "Authentication failed.");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    // Additional custom validation can be added here if needed
                    return Task.CompletedTask;
                },
                OnForbidden = context =>
                {
                    //var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtBearer");
                    logger.LogWarning("Forbidden: {Message}", context.Result?.ToString());
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtBearer");
                    logger.LogWarning("Challenge: {Message}", context.ErrorDescription);
                    return Task.CompletedTask;
                }
            };

        });

        services.AddAuthorizationBuilder()
            .AddPolicy("Bot", policy =>
            {
                //policy.RequireAssertion(_ => true);
                policy.RequireClaim("aud");
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("aud", [configuration[$"{aadConfigSectionName}:ClientId"]!]);
                //policy.Requirements.Add(new ScopeAuthorizationRequirement(["https://api.botframework.com/.default"]));
            })
            .AddPolicy("Agent", policy =>
            {
                policy.RequireAssertion(_ => true);
                //policy.RequireClaim("aud");
                //policy.RequireAuthenticatedUser();
                //policy.RequireClaim("aud", [configuration[$"{tokenValidationSectionName}:ClientId"]!]);
            });
    }
}
