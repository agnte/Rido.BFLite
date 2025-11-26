using Microsoft.Extensions.Logging;
using Rido.BFLite.Core.Schema;
using System.Text;

namespace Rido.BFLite.Core;

public class ConversationClient(
    HttpClient httpClient,
    ILogger<ConversationClient> logger)
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

        var agenticIdentity = AgenticIdentity.FromProperties(activity.From?.Properties!);

        string url = $"{activity.ServiceUrl!}v3/conversations/{activity.Conversation!.Id}/activities/";
        string body = activity.ToJson();

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

        // Pass the agentic identity to the handler via request options
        request.Options.Set(BotAuthenticationHandler.AgenticIdentityKey, agenticIdentity);

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("\n POST {url} \n\n", url);
            logger.LogTrace("Body: \n {Body} \n", body);
        }

        using HttpResponseMessage resp = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);

        return resp.IsSuccessStatusCode ?
            respContent :
            throw new Exception($"Error sending activity: {resp.StatusCode} - {respContent}");
    }
}
