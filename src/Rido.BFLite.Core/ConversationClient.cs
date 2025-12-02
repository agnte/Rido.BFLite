using Microsoft.Extensions.Logging;
using Rido.BFLite.Core.Hosting;
using Rido.BFLite.Core.Schema;
using System.ComponentModel;
using System.Text;

namespace Rido.BFLite.Core;

public class ConversationClient(HttpClient httpClient, ILogger<ConversationClient> logger)
{
    internal AgenticIdentity? AgenticIdentity { get; set; }

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

        AgenticIdentity? agenticIdentity = AgenticIdentity.FromProperties(activity.From?.Properties!);

        string url = $"{activity.ServiceUrl!}v3/conversations/{activity.Conversation!.Id}/activities/";
        string body = activity.ToJson();

        HttpRequestMessage request = new(HttpMethod.Post, url)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

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

    public async Task<IList<ConversationAccount>> GetConversationMembersAsync(string serviceUrl, string conversationId, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
    {

        string url = $"{serviceUrl}v3/conversations/{conversationId}/members";
        HttpRequestMessage request = new(HttpMethod.Get, url);
        if (customHeaders != null)
        {
            foreach (var header in customHeaders)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }
        request.Options.Set(BotAuthenticationHandler.AgenticIdentityKey, AgenticIdentity);
        using HttpResponseMessage resp = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);
        if (resp.IsSuccessStatusCode)
        {
            var members = System.Text.Json.JsonSerializer.Deserialize<IList<ConversationAccount>>(respContent);
            return members ?? new List<ConversationAccount>();
        }
        else
        {
            throw new Exception($"Error getting conversation members: {resp.StatusCode} - {respContent}");
        }
    }

    public async Task<IList<ConversationAccount>> GetConversationMemberAsync(string serviceUrl, string conversationId, string userId, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
    {

        string url = $"{serviceUrl}v3/conversations/{conversationId}/members/{userId}";
        HttpRequestMessage request = new(HttpMethod.Get, url);
        if (customHeaders != null)
        {
            foreach (var header in customHeaders)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }
        request.Options.Set(BotAuthenticationHandler.AgenticIdentityKey, AgenticIdentity);
        using HttpResponseMessage resp = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);
        if (resp.IsSuccessStatusCode)
        {
            var members = System.Text.Json.JsonSerializer.Deserialize<IList<ConversationAccount>>(respContent);
            return members ?? new List<ConversationAccount>();
        }
        else
        {
            throw new Exception($"Error getting conversation members: {resp.StatusCode} - {respContent}");
        }
    }

    public async Task<ConversationResource> CreateConversationAsync(string serviceUrl, CreateRequest createRequest, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
    {

        string url = $"{serviceUrl}v3/conversations";
        HttpRequestMessage request = new(HttpMethod.Post, url);
        request.Options.Set(BotAuthenticationHandler.AgenticIdentityKey, AgenticIdentity);
        if (customHeaders != null)
        {
            foreach (var header in customHeaders)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }
        request.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
        using HttpResponseMessage resp = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);
        if (resp.IsSuccessStatusCode)
        {
            var resource = System.Text.Json.JsonSerializer.Deserialize<ConversationResource>(respContent);
            return resource!;
        }
        else
        {
            throw new Exception($"Error getting conversation members: {resp.StatusCode} - {respContent}");
        }
    }

    public async Task DeleteActivityAsync(string serviceUrl, string conversationId, string activityId, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
    {
        string url = $"{serviceUrl}v3/conversations/{conversationId}/activities/{activityId}";
        HttpRequestMessage request = new(HttpMethod.Delete, url);
        if (customHeaders != null)
        {
            foreach (var header in customHeaders)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }
        request.Options.Set(BotAuthenticationHandler.AgenticIdentityKey, AgenticIdentity);
        using HttpResponseMessage resp = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);
        if (resp.IsSuccessStatusCode)
        {
            logger.LogInformation("Activity deleted successfully");
        }
        else
        {
            throw new Exception($"Error deleting activity: {resp.StatusCode} - {respContent}");
        }
    }

    public async Task DeleteConversationMemberAsync(string serviceUrl, string conversationId, string memberId, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
    {
        string url = $"{serviceUrl}v3/conversations/{conversationId}/members/{memberId}";
        HttpRequestMessage request = new(HttpMethod.Delete, url);
        if (customHeaders != null)
        {
            foreach (var header in customHeaders)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }
        request.Options.Set(BotAuthenticationHandler.AgenticIdentityKey, AgenticIdentity);
        using HttpResponseMessage resp = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);
        if (resp.IsSuccessStatusCode)
        {
            logger.LogInformation("Activity deleted successfully");
        }
        else
        {
            throw new Exception($"Error deleting activity: {resp.StatusCode} - {respContent}");
        }
    }

    public async Task<IList<ConversationAccount>> GetActivityMembersAsync(string serviceUrl, string conversationId, string activityId, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
    {
        string url = $"{serviceUrl}v3/conversations/{conversationId}/activities/{activityId}/members";
        HttpRequestMessage request = new(HttpMethod.Get, url);
        if (customHeaders != null)
        {
            foreach (var header in customHeaders)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }
        request.Options.Set(BotAuthenticationHandler.AgenticIdentityKey, AgenticIdentity);
        using HttpResponseMessage resp = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);
        if (resp.IsSuccessStatusCode)
        {
            var members = System.Text.Json.JsonSerializer.Deserialize<IList<ConversationAccount>>(respContent);
            return members ?? new List<ConversationAccount>();
        }
        else
        {
            throw new Exception($"Error getting activity members: {resp.StatusCode} - {respContent}");
        }
    }

    public async Task<ConversationResource> ReplyToActivityAsync(string serviceUrl, string conversationId, string activityId, Activity activity, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
    {
        string url = $"{serviceUrl}v3/conversations/{conversationId}/activities/{activityId}";
        HttpRequestMessage request = new(HttpMethod.Post, url);
        request.Options.Set(BotAuthenticationHandler.AgenticIdentityKey, AgenticIdentity);
        if (customHeaders != null)
        {
            foreach (var header in customHeaders)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }
        request.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(activity), Encoding.UTF8, "application/json");
        using HttpResponseMessage resp = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);
        if (resp.IsSuccessStatusCode)
        {
            var resource = System.Text.Json.JsonSerializer.Deserialize<ConversationResource>(respContent);
            return resource!;
        }
        else
        {
            throw new Exception($"Error replying to activity: {resp.StatusCode} - {respContent}");
        }
    }
}

public class CreateRequest
{
    [JsonPropertyName("isGroup")]
    public bool? IsGroup { get; set; }

    [JsonPropertyName("bot")]
    public ConversationAccount? Bot { get; set; }

    [JsonPropertyName("members")]
    public IList<ConversationAccount>? Members { get; set; }

    [JsonPropertyName("topicName")]
    public string? TopicName { get; set; }

    [JsonPropertyName("tenantId")]
    public string? TenantId { get; set; }

    [JsonPropertyName("activity")]
    public Activity? Activity { get; set; }

    [JsonPropertyName("channelData")]
    public ExtendedPropertiesDictionary? ChannelData { get; set; }
}

public class ConversationResource
{
    /// <summary>
    /// Id of the resource
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// ID of the Activity (if sent)
    /// </summary>
    [JsonPropertyName("activityId")]
    public string? ActivityId { get; set; }

    /// <summary>
    /// Service endpoint where operations concerning the conversation may be performed
    /// </summary>
    [JsonPropertyName("serviceUrl")]
    public string? ServiceUrl { get; set; }
}
