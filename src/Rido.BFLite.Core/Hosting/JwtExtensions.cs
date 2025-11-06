using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Validators;

namespace Rido.BFLite.Core.Hosting;


public static class JwtExtensions
{
    public static AuthenticationBuilder AddBotAuthentication(this IServiceCollection services, string aadSectionName = "AzureAd")
    {
        var authenticationBuilder = services.AddAuthentication();
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        //string agentScope = configuration[$"{aadSectionName}:AgentScope"]!;
        string audience = configuration[$"{aadSectionName}:ClientId"]!;
        string tenantId = configuration[$"{aadSectionName}:TenantId"]!;

        services
            .AddAuthentication()
            .AddCustomJwtBearer("Bot", "botframework.com", audience)
            .AddCustomJwtBearer("Agent", tenantId, audience);
        return authenticationBuilder;
    }

    public static AuthenticationBuilder AddBotAuthenticationEx(this IServiceCollection services, IEnumerable<string> aadSectionNames)
    {
        var authenticationBuilder = services.AddAuthentication();
        foreach (var aadSectionName in aadSectionNames)
        {
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            string agentScope = configuration[$"{aadSectionName}:AgentScope"]!;
            string audience = configuration[$"{aadSectionName}:ClientId"]!;
            string tenantId = configuration[$"{aadSectionName}:TenantId"]!;
            bool isBot = agentScope.Equals("https://api.botframework.com/.default", StringComparison.OrdinalIgnoreCase);

            if (isBot)
            {;
                authenticationBuilder.AddCustomJwtBearer(aadSectionName + "_Bot", "botframework.com", audience);
            }
            else
            {

                authenticationBuilder.AddCustomJwtBearer(aadSectionName + "_Agent", tenantId, audience);
            }

        }
        return authenticationBuilder;
    }

    public static AuthorizationBuilder AddBotAuthorization(this IServiceCollection services)
    {
        var authorizationBuilder = services
            .AddAuthorizationBuilder()
            .AddDefaultPolicy("DefaultPolicy", policy =>
            {
                policy.AuthenticationSchemes.Add("Bot");
                policy.AuthenticationSchemes.Add("Agent");
                policy.RequireAuthenticatedUser();
            });
        return authorizationBuilder;
    }

    public static AuthorizationBuilder AddBotAuthorizationEx(this IServiceCollection services, IEnumerable<string> aadSectionNames)
    {
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

        var authorizationBuilder = services.AddAuthorizationBuilder();
        foreach (var aadSectionName in aadSectionNames)
        {
            string agentScope = configuration[$"{aadSectionName}:AgentScope"]!;
            bool isBot = agentScope.Equals("https://api.botframework.com/.default", StringComparison.OrdinalIgnoreCase);
            authorizationBuilder = authorizationBuilder.AddDefaultPolicy("DefaultPolicy", policy =>
            {
                if (isBot)
                {
                    policy.AuthenticationSchemes.Add(aadSectionName + "_Bot");
                }
                else
                {
                    policy.AuthenticationSchemes.Add(aadSectionName + "_Agent");
                }
                policy.RequireAuthenticatedUser();
            });
        }
        return authorizationBuilder;
    }

    public static AuthenticationBuilder AddCustomJwtBearer(this AuthenticationBuilder builder, string schemeName, string tenantId, string audience)
    {
        string metadataAddress = tenantId.Equals("botframework.com", StringComparison.OrdinalIgnoreCase)
            ? "https://login.botframework.com/v1/.well-known/openidconfiguration"
            : $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration";

        string[] validIssuers = tenantId.Equals("botframework.com", StringComparison.OrdinalIgnoreCase)
            ? ["https://api.botframework.com"]
            : [$"https://sts.windows.net/{tenantId}/", $"https://login.microsoftonline.com/{tenantId}/v2", "https://api.botframework.com"];

        builder.AddJwtBearer(schemeName, jwtOptions =>
         {
             jwtOptions.SaveToken = true;
             jwtOptions.IncludeErrorDetails = true;
             jwtOptions.MetadataAddress = metadataAddress;
             jwtOptions.Audience = audience;
             jwtOptions.TokenValidationParameters = new TokenValidationParameters
             {
                 ValidateIssuerSigningKey = true,
                 RequireSignedTokens = true,
                 ValidateIssuer = true,
                 ValidateAudience = true,
                 ValidIssuers = validIssuers
             };
             jwtOptions.TokenValidationParameters.EnableAadSigningKeyIssuerValidation();
             jwtOptions.MapInboundClaims = true;
             // jwtOptions.Events = jwtEvents;
             jwtOptions.Validate();
         });
        return builder;
    }

    readonly static JwtBearerEvents jwtEvents = new()
    {
        OnMessageReceived = context =>
        {
            string accessToken = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last()!;
            return Task.CompletedTask;
        },
        OnForbidden = context =>
        {
            var f = context.Principal;
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var ex = context.Exception;
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.ToString());
            return System.Threading.Tasks.Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var v = context.SecurityToken;
            Console.WriteLine("Token validated");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine("token challenged");
            var error = context.Error;
            return Task.CompletedTask;
        }
    };
}
