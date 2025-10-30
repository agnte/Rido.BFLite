using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using System.Security.Claims;

Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

string agentScope = "0d94caae-b412-4943-8a68-83135ad6d35f/.default";
//string tenantId = "5369a35c-46a5-4677-8ff9-2e65587654e7";
//string clientId = "9d17cc32-c91b-4368-8494-1b29ccb0dbcf";
//string miClientId = "a8c6bf8e-d4a0-4181-b91d-501314cda52a";
string agentIdentity = "5081ddac-3d33-4766-98fe-80c38c5ce554";
string userId = "96a64abd-3267-4143-b9a5-194e6f96ef2b";

var builder = new HostApplicationBuilder(args);
builder.Services.AddTokenAcquisition(true);
builder.Services.AddAgentIdentities();
builder.Services.AddInMemoryTokenCaches();
builder.Services.AddHttpClient();
var app = builder.Build();

string[] scopes = new[] { agentScope };

IAuthorizationHeaderProvider authorizationHeaderProvider = app.Services.GetRequiredService<IAuthorizationHeaderProvider>();
AuthorizationHeaderProviderOptions options = new AuthorizationHeaderProviderOptions().WithAgentUserIdentity(agentIdentity, Guid.Parse(userId));

ClaimsPrincipal user = new ClaimsPrincipal();

string authHeader = await authorizationHeaderProvider
    .CreateAuthorizationHeaderAsync(scopes,options,cancellationToken: default);


user.Claims.ToList().ForEach(c => System.Console.WriteLine($"{c.Type}: {c.Value}"));
System.Console.WriteLine(authHeader);
