using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.JsonWebTokens;
using Rido.BFLite.Core.Schema;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Rido.BFLite.Core;

public class ConversationClient(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ILogger<ConversationClient>
    logger, IAuthorizationHeaderProvider authorizationHeaderProvider)
{
    private async Task<HttpClient> CreateAuthenticatedHttpClientAsync(Activity activity, CancellationToken cancellationToken = default)
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

        if (logger.IsEnabled(LogLevel.Trace))
        {
            var jsonWebToken = new JsonWebToken(tokenValue);
            logger.LogTrace("Token Claims : \n {claims}", string.Join("\n ", jsonWebToken.Claims.Select(c => $"{c.Type}: {c.Value}")));
        }

        return httpClient;
    }

    public async Task<ResourceResponse> SendActivityAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = await CreateAuthenticatedHttpClientAsync(activity, cancellationToken);

        string url = $"{activity.ServiceUrl!}v3/conversations/{activity.Conversation!.Id}/activities/";
        string body = activity.ToJson();

        logger.LogTrace("Sending response to \n POST {url} \n\n {body} \n\n", url, body);

        using HttpResponseMessage resp = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        }, cancellationToken);

        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);

        if (!resp.IsSuccessStatusCode)
        {
            throw new Exception($"Error sending activity: {resp.StatusCode} - {respContent}");
        }

        return JsonSerializer.Deserialize<ResourceResponse>(respContent, Activity.DefaultJsonOptions) ?? new ResourceResponse();
    }

    public async Task<ResourceResponse> ReplyToActivityAsync(Activity activity, string replyToId, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = await CreateAuthenticatedHttpClientAsync(activity, cancellationToken);

        Uri serviceUri = new(activity.ServiceUrl!);
        string url = $"{serviceUri.Scheme}://{serviceUri.Host}/v3/conversations/{activity.Conversation!.Id}/activities/{replyToId}";
        string body = activity.ToJson();

        logger.LogTrace("Sending reply to \n POST {url} \n\n {body} \n\n", url, body);

        using HttpResponseMessage resp = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        }, cancellationToken);

        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);

        if (!resp.IsSuccessStatusCode)
        {
            throw new Exception($"Error replying to activity: {resp.StatusCode} - {respContent}");
        }

        return JsonSerializer.Deserialize<ResourceResponse>(respContent, Activity.DefaultJsonOptions) ?? new ResourceResponse();
    }

    public async Task<ResourceResponse> UpdateActivityAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = await CreateAuthenticatedHttpClientAsync(activity, cancellationToken);

        Uri serviceUri = new(activity.ServiceUrl!);
        string url = $"{serviceUri.Scheme}://{serviceUri.Host}/v3/conversations/{activity.Conversation!.Id}/activities/{activity.Id}";
        string body = activity.ToJson();

        logger.LogTrace("Updating activity at \n PUT {url} \n\n {body} \n\n", url, body);

        using HttpResponseMessage resp = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        }, cancellationToken);

        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);

        if (!resp.IsSuccessStatusCode)
        {
            throw new Exception($"Error updating activity: {resp.StatusCode} - {respContent}");
        }

        return JsonSerializer.Deserialize<ResourceResponse>(respContent, Activity.DefaultJsonOptions) ?? new ResourceResponse();
    }

    public async Task<ResourceResponse> DeleteActivityAsync(Activity activity, string activityId, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = await CreateAuthenticatedHttpClientAsync(activity, cancellationToken);

        Uri serviceUri = new(activity.ServiceUrl!);
        string url = $"{serviceUri.Scheme}://{serviceUri.Host}/v3/conversations/{activity.Conversation!.Id}/activities/{activityId}";

        logger.LogTrace("Deleting activity at \n DELETE {url}", url);

        using HttpResponseMessage resp = await httpClient.DeleteAsync(url, cancellationToken);

        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);

        if (!resp.IsSuccessStatusCode)
        {
            throw new Exception($"Error deleting activity: {resp.StatusCode} - {respContent}");
        }

        return JsonSerializer.Deserialize<ResourceResponse>(respContent, Activity.DefaultJsonOptions) ?? new ResourceResponse();
    }

    public async Task<ConversationResourceResponse> CreateConversationAsync(Activity activity, ConversationParameters parameters, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = await CreateAuthenticatedHttpClientAsync(activity, cancellationToken);

        Uri serviceUri = new(activity.ServiceUrl!);
        string url = $"{serviceUri.Scheme}://{serviceUri.Host}/v3/conversations";
        string body = JsonSerializer.Serialize(parameters, Activity.DefaultJsonOptions);

        logger.LogTrace("Creating conversation at \n POST {url} \n\n {body} \n\n", url, body);

        using HttpResponseMessage resp = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        }, cancellationToken);

        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);

        if (!resp.IsSuccessStatusCode)
        {
            throw new Exception($"Error creating conversation: {resp.StatusCode} - {respContent}");
        }

        return JsonSerializer.Deserialize<ConversationResourceResponse>(respContent, Activity.DefaultJsonOptions) ?? new ConversationResourceResponse();
    }

    public async Task<List<ConversationAccount>> GetConversationMembersAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = await CreateAuthenticatedHttpClientAsync(activity, cancellationToken);

        Uri serviceUri = new(activity.ServiceUrl!);
        string url = $"{serviceUri.Scheme}://{serviceUri.Host}/v3/conversations/{activity.Conversation!.Id}/members";

        logger.LogTrace("Getting conversation members at \n GET {url}", url);

        using HttpResponseMessage resp = await httpClient.GetAsync(url, cancellationToken);

        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);

        if (!resp.IsSuccessStatusCode)
        {
            throw new Exception($"Error getting conversation members: {resp.StatusCode} - {respContent}");
        }

        return JsonSerializer.Deserialize<List<ConversationAccount>>(respContent, Activity.DefaultJsonOptions) ?? new List<ConversationAccount>();
    }

    public async Task<PagedMembersResult> GetConversationPagedMembersAsync(Activity activity, int? pageSize = null, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = await CreateAuthenticatedHttpClientAsync(activity, cancellationToken);

        Uri serviceUri = new(activity.ServiceUrl!);
        string queryParams = "";
        if (pageSize.HasValue || !string.IsNullOrEmpty(continuationToken))
        {
            List<string> queryParts = new();
            if (pageSize.HasValue)
            {
                queryParts.Add($"pageSize={pageSize.Value}");
            }
            if (!string.IsNullOrEmpty(continuationToken))
            {
                queryParts.Add($"continuationToken={Uri.EscapeDataString(continuationToken)}");
            }
            queryParams = "?" + string.Join("&", queryParts);
        }
        string url = $"{serviceUri.Scheme}://{serviceUri.Host}/v3/conversations/{activity.Conversation!.Id}/pagedmembers{queryParams}";

        logger.LogTrace("Getting paged conversation members at \n GET {url}", url);

        using HttpResponseMessage resp = await httpClient.GetAsync(url, cancellationToken);

        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);

        if (!resp.IsSuccessStatusCode)
        {
            throw new Exception($"Error getting paged conversation members: {resp.StatusCode} - {respContent}");
        }

        return JsonSerializer.Deserialize<PagedMembersResult>(respContent, Activity.DefaultJsonOptions) ?? new PagedMembersResult();
    }

    public async Task<List<ConversationAccount>> GetActivityMembersAsync(Activity activity, string activityId, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = await CreateAuthenticatedHttpClientAsync(activity, cancellationToken);

        Uri serviceUri = new(activity.ServiceUrl!);
        string url = $"{serviceUri.Scheme}://{serviceUri.Host}/v3/conversations/{activity.Conversation!.Id}/activities/{activityId}/members";

        logger.LogTrace("Getting activity members at \n GET {url}", url);

        using HttpResponseMessage resp = await httpClient.GetAsync(url, cancellationToken);

        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);

        if (!resp.IsSuccessStatusCode)
        {
            throw new Exception($"Error getting activity members: {resp.StatusCode} - {respContent}");
        }

        return JsonSerializer.Deserialize<List<ConversationAccount>>(respContent, Activity.DefaultJsonOptions) ?? new List<ConversationAccount>();
    }

    public async Task<ResourceResponse> UploadAttachmentAsync(Activity activity, object attachmentUpload, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = await CreateAuthenticatedHttpClientAsync(activity, cancellationToken);

        Uri serviceUri = new(activity.ServiceUrl!);
        string url = $"{serviceUri.Scheme}://{serviceUri.Host}/v3/conversations/{activity.Conversation!.Id}/attachments";
        string body = JsonSerializer.Serialize(attachmentUpload, Activity.DefaultJsonOptions);

        logger.LogTrace("Uploading attachment at \n POST {url} \n\n {body} \n\n", url, body);

        using HttpResponseMessage resp = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        }, cancellationToken);

        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);

        if (!resp.IsSuccessStatusCode)
        {
            throw new Exception($"Error uploading attachment: {resp.StatusCode} - {respContent}");
        }

        return JsonSerializer.Deserialize<ResourceResponse>(respContent, Activity.DefaultJsonOptions) ?? new ResourceResponse();
    }
}
