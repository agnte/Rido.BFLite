using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using System.Security.Claims;

Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

string agentScope = "5a807f24-c9de-44ee-a3a7-329e88a00ffc/.default";
string tenantId = "e8a85347-fb53-4a91-9267-c616cbe1fd16";
string clientId = "f8c0696d-bb99-4bb4-acc9-47c734d91e91";
//string miClientId = "a8c6bf8e-d4a0-4181-b91d-501314cda52a";
string agentIdentity = "f30805e3-3457-4c6e-a0e7-bf0fd623f887";
string userId = "715d0396-3a7a-4d44-800d-225d04e4d510";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTokenAcquisition(true);
builder.Services.AddAgentIdentities();
builder.Services.AddInMemoryTokenCaches();
builder.Services.AddHttpClient();

builder.Services.Configure<MicrosoftIdentityApplicationOptions>(builder.Configuration.GetSection("AzureAd"));

// string secret = builder.Configuration["MicrosoftAppPassword"]!;
//builder.Services.Configure<MicrosoftIdentityApplicationOptions>(ops =>
//{
//    ops.Instance = "https://login.microsoftonline.com/";
//    ops.TenantId = tenantId;
//    ops.ClientId = clientId;
//    ops.ClientCredentials = [
//        new CredentialDescription()
//        {
//            //SourceType = CredentialSource.SignedAssertionFromManagedIdentity,
//            //ManagedIdentityClientId = miClientId
//            SourceType = CredentialSource.ClientSecret,
//            ClientSecret = secret 
//        }
//    ];
//});
var app = builder.Build();

string[] scopes = new[] { agentScope };

IAuthorizationHeaderProvider authorizationHeaderProvider = app.Services.GetRequiredService<IAuthorizationHeaderProvider>();
AuthorizationHeaderProviderOptions options = new AuthorizationHeaderProviderOptions()
    .WithAgentUserIdentity(agentIdentity, Guid.Parse(userId));

ClaimsPrincipal user = new ClaimsPrincipal();

string authHeader = await authorizationHeaderProvider
    .CreateAuthorizationHeaderAsync(scopes,options,user, cancellationToken: default);


user.Claims.ToList().ForEach(c => System.Console.WriteLine($"{c.Type}: {c.Value}"));

Console.WriteLine(  );

var jsonToken = new Microsoft.IdentityModel.JsonWebTokens.JsonWebToken(authHeader["Bearer ".Length..]);
jsonToken.Claims.ToList().ForEach(c => System.Console.WriteLine($"{c.Type}: {c.Value}"));

System.Console.WriteLine(authHeader);
