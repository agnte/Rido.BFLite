using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensibility;
using System.Security.Claims;

namespace Rido.BFLite.Core;

public class AgenticCredentialsProvider(IConfiguration configuration, string tokenValidationSectionName = "AzureAd") : IAuthorizationHeaderProvider
{
    string tenantId = configuration[$"{tokenValidationSectionName}:ClientCredentials:0:TenantId"] ?? throw new ArgumentNullException($"{tokenValidationSectionName}:ClientCredentials:0:TenantId");
    string clientId = configuration[$"{tokenValidationSectionName}:ClientCredentials:0:ClientId"] ?? throw new ArgumentNullException($"{tokenValidationSectionName}:ClientCredentials:0:ClientId");
    string secret = configuration[$"{tokenValidationSectionName}:ClientCredentials:0:ClientSecret"] ?? throw new ArgumentNullException($"{tokenValidationSectionName}:ClientCredentials:0:ClientSecret");
    string fmiPath = configuration[$"{tokenValidationSectionName}:ClientCredentials:0:FmiPath"] ?? throw new ArgumentNullException($"{tokenValidationSectionName}:ClientCredentials:0:FmiPath");
    string userId = configuration[$"{tokenValidationSectionName}:UserId"] ?? throw new ArgumentNullException($"{tokenValidationSectionName}:UserId");
    string agentScope = configuration[$"{tokenValidationSectionName}:AgentScope"] ?? throw new ArgumentNullException($"{tokenValidationSectionName}:AgentScope");
    //string fmiPath = configuration[$"{tokenValidationSectionName}:ABP"] ?? throw new ArgumentNullException("ABP");

    public Task<string> CreateAuthorizationHeaderAsync(IEnumerable<string> scopes, AuthorizationHeaderProviderOptions? options = null, ClaimsPrincipal? claimsPrincipal = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<string> CreateAuthorizationHeaderForAppAsync(string scopes, AuthorizationHeaderProviderOptions? downstreamApiOptions = null, CancellationToken cancellationToken = default)
    {
        string authority = $"https://login.microsoftonline.com/{tenantId}";

        IList<string> scopesAD = new[] { "api://AzureADTokenExchange/.default" };


        var agentAppClient = ConfidentialClientApplicationBuilder
           .Create(clientId)
           .WithAuthority(authority)
           .WithClientSecret(secret) // Use the managed identity token as the client assertion
           .Build();

        var result = await agentAppClient.AcquireTokenForClient(scopesAD)
            .WithFmiPath(fmiPath)
            .ExecuteAsync(cancellationToken);

#pragma warning disable CS0618 // Type or member is obsolete
        IConfidentialClientApplication agentIdentityClient = ConfidentialClientApplicationBuilder
                        .Create(fmiPath)
                        .WithClientAssertion(result.AccessToken)
                        .Build();
#pragma warning restore CS0618 // Type or member is obsolete

        AuthenticationResult agentAuthResult = await agentIdentityClient.AcquireTokenForClient(scopesAD)
                        .WithTenantId(tenantId)
                        .ExecuteAsync(cancellationToken);

#pragma warning disable CS0618 // Type or member is obsolete
        IByUsernameAndPassword cca = (IByUsernameAndPassword)ConfidentialClientApplicationBuilder
                                .Create(fmiPath)
                                .WithTenantId(tenantId)
                                .WithClientAssertion(result.AccessToken)
                                .Build();
#pragma warning restore CS0618 // Type or member is obsolete


        //string[] apxScope = new string[] { "0d94caae-b412-4943-8a68-83135ad6d35f/.default" };
        //IList<string> apxScope = ["5a807f24-c9de-44ee-a3a7-329e88a00ffc/.default"];
        IList<string> apxScope = [agentScope];

        AuthenticationResult userToken = await cca
                        .AcquireTokenByUsernamePassword(apxScope, "no-user-name", "no_password")
                        .OnBeforeTokenRequest(async request =>
                        {
                            string userFicAssertion = agentAuthResult.AccessToken;
                            request.BodyParameters["user_federated_identity_credential"] = userFicAssertion;
                            request.BodyParameters["grant_type"] = "user_fic";
                            request.BodyParameters["user_id"] = userId;

                            // request.BodyParameters["client_assertion_type"] = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";

                            // remove the password
                            request.BodyParameters.Remove("password");
                            request.BodyParameters.Remove("username");

                            if (request.BodyParameters.TryGetValue("client_secret", out var secret)
                                    && secret.Equals("default", StringComparison.OrdinalIgnoreCase))
                            {
                                request.BodyParameters.Remove("client_secret");
                            }
                            await Task.CompletedTask;
                        }
                        )
                        .ExecuteAsync(cancellationToken)
                        .ConfigureAwait(false);

        return userToken.AccessToken;
    }

    public async Task<string> CreateAuthorizationHeaderForUserAsync(IEnumerable<string> scopes2, AuthorizationHeaderProviderOptions? authorizationHeaderProviderOptions = null, ClaimsPrincipal? claimsPrincipal = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
