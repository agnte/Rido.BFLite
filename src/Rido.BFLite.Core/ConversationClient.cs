using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.JsonWebTokens;
using Rido.BFLite.Core.Schema;
using System.Net.Http.Headers;
using System.Text;

namespace Rido.BFLite.Core;

public class ConversationClient(IHttpClientFactory httpClientFactory, IAuthorizationHeaderProvider authorizationHeaderProvider, ILogger<ConversationClient> logger, IConfiguration configuration)
{
    string tenantId = configuration["AzureAd:AgentTenantId"]!;
    public async Task<string> SendActivityAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        string agentScope = configuration["AzureAd:AgentScope"]!;
        activity.From!.Properties.TryGetValue("agenticAppId", out object? agenticAppId);
        activity.From!.Properties.TryGetValue("agenticUserId", out object? agenticUserId);
        activity.From!.Properties.TryGetValue("tenantId", out object? tenantId);

        using HttpClient httpClient = httpClientFactory.CreateClient();
        AuthorizationHeaderProviderOptions options = new AuthorizationHeaderProviderOptions();
        string token;
        if (agentScope != "https://api.botframework.com/.default" && agenticAppId is not null && agenticUserId is not null)
        {
            options.WithAgentUserIdentity(agenticAppId.ToString()!, Guid.Parse(agenticUserId.ToString()!));
            token = await authorizationHeaderProvider.CreateAuthorizationHeaderAsync([agentScope], options, null, cancellationToken);
        }
        else
        {
            token = await authorizationHeaderProvider.CreateAuthorizationHeaderForAppAsync(agentScope, options, cancellationToken);
        }

        string tokenValue = token["Bearer ".Length..];
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);

        string url = $"{activity.ServiceUrl!}v3/conversations/{activity.Conversation!.Id}/activities/";
        string body = activity.ToJson();

        if (logger.IsEnabled(LogLevel.Trace))
        {
            var jsonWebToken = new JsonWebToken(tokenValue);

            // File.WriteAllText($"out_act_{activity.Id!}.json", body);
            logger.LogTrace("\n POST {url} \n\n", url);
            logger.LogTrace("Token Claims : \n {claims}", string.Join("\n ", jsonWebToken.Claims.Select(c => $"{c.Type}: {c.Value}")));
            logger.LogTrace("Body: \n {Body} \n", body);
        }

        using HttpResponseMessage resp = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        }, cancellationToken);

        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);

        return resp.IsSuccessStatusCode ?
            respContent :
            throw new Exception($"Error sending activity: {resp.StatusCode} - {respContent}");
    }
}
