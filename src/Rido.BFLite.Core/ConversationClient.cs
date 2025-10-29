using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Abstractions;
using Rido.BFLite.Core.Schema;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Rido.BFLite.Core;

public class ConversationClient(IHttpClientFactory httpClientFactory, IAuthorizationHeaderProvider tokenProvider, ILogger<ConversationClient> logger, IConfiguration configuration)
{
    private async Task<HttpClient> CreateAuthenticatedHttpClientAsync(CancellationToken cancellationToken = default)
    {
        string outTenantId = configuration["AzureAD:ClientCredentials:0:TenantId"]!;
        string scope = "https://api.botframework.com/.default";
        HttpClient httpClient = httpClientFactory.CreateClient();
        string token = await tokenProvider!.CreateAuthorizationHeaderForAppAsync(
            scope,
            new AuthorizationHeaderProviderOptions() { RequestAppToken = true, AcquireTokenOptions = new AcquireTokenOptions() { Tenant = outTenantId } },
            cancellationToken);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token["Bearer ".Length..]);
        return httpClient;
    }

    public async Task<ResourceResponse> SendActivityAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = await CreateAuthenticatedHttpClientAsync(cancellationToken);

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

        if (!resp.IsSuccessStatusCode)
        {
            throw new Exception($"Error sending activity: {resp.StatusCode} - {respContent}");
        }

        return JsonSerializer.Deserialize<ResourceResponse>(respContent, Activity.DefaultJsonOptions) ?? new ResourceResponse();
    }

    public async Task<ResourceResponse> ReplyToActivityAsync(Activity activity, string replyToId, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = await CreateAuthenticatedHttpClientAsync(cancellationToken);

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
        using HttpClient httpClient = await CreateAuthenticatedHttpClientAsync(cancellationToken);

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

    public async Task DeleteActivityAsync(string serviceUrl, string conversationId, string activityId, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = await CreateAuthenticatedHttpClientAsync(cancellationToken);

        Uri serviceUri = new(serviceUrl);
        string url = $"{serviceUri.Scheme}://{serviceUri.Host}/v3/conversations/{conversationId}/activities/{activityId}";

        logger.LogTrace("Deleting activity at \n DELETE {url}", url);

        using HttpResponseMessage resp = await httpClient.DeleteAsync(url, cancellationToken);

        string respContent = await resp.Content.ReadAsStringAsync(cancellationToken);
        logger.LogTrace("Response Status {status}, content {content}", resp.StatusCode, respContent);

        if (!resp.IsSuccessStatusCode)
        {
            throw new Exception($"Error deleting activity: {resp.StatusCode} - {respContent}");
        }
    }

    public async Task<ConversationResourceResponse> CreateConversationAsync(string serviceUrl, ConversationParameters parameters, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = await CreateAuthenticatedHttpClientAsync(cancellationToken);

        Uri serviceUri = new(serviceUrl);
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

    public async Task<List<ConversationAccount>> GetConversationMembersAsync(string serviceUrl, string conversationId, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = await CreateAuthenticatedHttpClientAsync(cancellationToken);

        Uri serviceUri = new(serviceUrl);
        string url = $"{serviceUri.Scheme}://{serviceUri.Host}/v3/conversations/{conversationId}/members";

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

    public async Task<PagedMembersResult> GetConversationPagedMembersAsync(string serviceUrl, string conversationId, int? pageSize = null, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = await CreateAuthenticatedHttpClientAsync(cancellationToken);

        Uri serviceUri = new(serviceUrl);
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
        string url = $"{serviceUri.Scheme}://{serviceUri.Host}/v3/conversations/{conversationId}/pagedmembers{queryParams}";

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

    public async Task<List<ConversationAccount>> GetActivityMembersAsync(string serviceUrl, string conversationId, string activityId, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = await CreateAuthenticatedHttpClientAsync(cancellationToken);

        Uri serviceUri = new(serviceUrl);
        string url = $"{serviceUri.Scheme}://{serviceUri.Host}/v3/conversations/{conversationId}/activities/{activityId}/members";

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

    public async Task<ResourceResponse> UploadAttachmentAsync(string serviceUrl, string conversationId, object attachmentUpload, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = await CreateAuthenticatedHttpClientAsync(cancellationToken);

        Uri serviceUri = new(serviceUrl);
        string url = $"{serviceUri.Scheme}://{serviceUri.Host}/v3/conversations/{conversationId}/attachments";
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
