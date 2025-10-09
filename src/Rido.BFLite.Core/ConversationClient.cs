using Microsoft.Extensions.Logging;
using Microsoft.Identity.Abstractions;
using Rido.BFLite.Core.Schema;
using System.Net.Http.Headers;
using System.Text;

namespace Rido.BFLite.Core;

public class ConversationClient(IHttpClientFactory httpClientFactory, IAuthorizationHeaderProvider tokenProvider, ILogger<ConversationClient> logger)
{
    public async Task<string> SendActivityAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = httpClientFactory.CreateClient();
        string token = await tokenProvider!.CreateAuthorizationHeaderForAppAsync(
            "https://api.botframework.com/.default",
            new AuthorizationHeaderProviderOptions() { RequestAppToken = true, AcquireTokenOptions = new AcquireTokenOptions() { Tenant = "botframework.com" } },
            cancellationToken);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token["Bearer ".Length..]);

        Uri serviceUri = new(activity.ServiceUrl!);
        string url = $"{serviceUri.Scheme}://{serviceUri.Host}/v3/conversations/{activity.Conversation!.Id}/activities";
        string body = activity.ToJson();

        logger.LogTrace("Sending response to \n POST {url} \n\n {body} \n\n", url, body);

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
