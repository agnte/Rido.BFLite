using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Validators;

namespace Rido.BFLite.Core.Hosting;


public static class JwtExtensions
{

    public static AuthenticationBuilder AddBotAgentAuthentication(this AuthenticationBuilder builder, IConfiguration configuration, string aadSectionName = "AzureAd")
    {
        string agentScope = configuration[$"{aadSectionName}:AgentScope"]!;
        string audience = configuration[$"{aadSectionName}:ClientId"]!;
        if (string.IsNullOrEmpty(agentScope) || agentScope.Equals("https://api.botframework.com/.default", StringComparison.OrdinalIgnoreCase)) {

            builder.AddCustomJwtBearer("Bot", "botframework.com", audience);
        }
        else
        {
            string tenantId = configuration[$"{aadSectionName}:TenantId"]!;
            builder.AddCustomJwtBearer("Agent", tenantId, audience);
        }
        return builder;
    }

    public static AuthenticationBuilder AddCustomJwtBearer(this AuthenticationBuilder builder, string schemeName, string tenantId, string audience)
    {
        string metadataAddress = tenantId.Equals("botframework.com", StringComparison.OrdinalIgnoreCase)
            ? "https://login.botframework.com/v1/.well-known/openidconfiguration"
            : $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration";

        string[] validIssuers = tenantId.Equals("botframework.com", StringComparison.OrdinalIgnoreCase)
            ? ["https://api.botframework.com"]
            : [$"https://sts.windows.net/{tenantId}/", $"https://login.microsoftonline.com/{tenantId}/v2"];

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
             //jwtOptions.Events = jwtEvents;
             jwtOptions.Validate();
         });
        return builder;
    }

    readonly static JwtBearerEvents jwtEvents = new JwtBearerEvents
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
