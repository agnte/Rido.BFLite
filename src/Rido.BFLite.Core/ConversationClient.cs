using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Rido.BFLite.Core.Schema;
using System.Net.Http.Headers;
using System.Text;

namespace Rido.BFLite.Core;

public class ConversationClient(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ILogger<ConversationClient> logger,
    AgentAuthorizationHeaderProviderService tokenService,
    string aadConfigSectionName = "AzureAd")
{
    public async Task<string> SendActivityAsync(Activity activity, CancellationToken cancellationToken = default)
    {

        if (activity.Type == "trace")
        {
            logger.LogTrace("Skipping trace activity {activityId}", activity.Id);
            return string.Empty;
        }

        if (activity.Type.Contains("invoke", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogTrace("Skipping invoke activity {activityId}", activity.Id);
            return string.Empty;
        }

        string agentScope = configuration[$"{aadConfigSectionName}:AgentScope"]!;

        var agenticIdentity = AgenticIdentity.FromProperties(activity.From?.Properties!); 
        string token = await tokenService.GetAuthorizationHeaderAsync(agentScope, agenticIdentity, aadConfigSectionName, cancellationToken).ConfigureAwait(false);
        string tokenValue = token.StartsWith("Bearer ") ? token["Bearer ".Length..] : token;

        using HttpClient httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);

        string url = $"{activity.ServiceUrl!}v3/conversations/{activity.Conversation!.Id}/activities/";
        string body = activity.ToJson();

        if (logger.IsEnabled(LogLevel.Trace))
        {
            var jsonWebToken = new JsonWebToken(tokenValue);

            logger.LogTrace("\n POST {url} \n\n", url);
            logger.LogTrace("Token Claims : \n {claims}", string.Join("\n ", jsonWebToken.Claims.Select(c => $"{c.Type}: {c.Value}")));
            logger.LogTrace("Body: \n {Body} \n", body);
        }

        using HttpResponseMessage resp = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        }, cancellationToken).ConfigureAwait(false);

        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);

        return resp.IsSuccessStatusCode ?
            respContent :
            throw new Exception($"Error sending activity: {resp.StatusCode} - {respContent}");
    }
}
