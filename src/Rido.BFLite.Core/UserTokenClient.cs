using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Abstractions;
using Rido.BFLite.Core.Schema;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Rido.BFLite.Core;

public interface IUserTokenClient
{
    public class GetTokenStatusResult
    {
        public string? ConnectionName { get; set; }
        public bool? HasToken { get; set; }
        public string? ServiceProviderDisplayName { get; set; }
    }


    public class GetSignInResourceResult
    {
        public Signinresource? SignInResource { get; set; }
    }

    public class Signinresource
    {
        public string? SignInLink { get; set; }
        public Tokenpostresource? TokenPostResource { get; set; }
    }

    public class Tokenpostresource
    {
        public string? SasUrl { get; set; }
    }
    public class GetTokenResult
    {
        public string? ConnectionName { get; set; }
        public string? Token { get; set; }
        //public int ExpiresIn { get; set; }
        //public string? ExpirationTime { get; set; }
    }

    /// <summary>
    /// Gets the user token for a particular connection.
    /// </summary>
    /// <returns>The token result, or null if the token is not found.</returns>
    Task<GetTokenResult> GetTokenAsync(string userId, string connectionName, string channelId, string? code = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the raw signin link to be sent to the user for signin for a connection.
    /// </summary>
    Task<GetSignInResourceResult> GetTokenOrSignInResource(string userId, string connectionName, string channelId, string? finalRedirect = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the token status for each connection for the given user.
    /// </summary>
    Task<GetTokenStatusResult> GetTokenStatusAsync(string userId, string channelId, string? include = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Signs the user out of a connection.
    /// </summary>
    Task<bool> SignOutUserAsync(string userId, string? connectionName = null, string? channelId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchanges a token for another token.
    /// </summary>
    Task<string> ExchangeTokenAsync(string userId, string connectionName, string channelId, string exchangeToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets AAD tokens for a user.
    /// </summary>
    Task<string> GetAadTokensAsync(string userId, string connectionName, string channelId, string[]? resourceUrls = null, CancellationToken cancellationToken = default);

    public AgenticIdentity? AgenticIdentity { get; set; }
}

public class UserTokenClient(
    ILogger<UserTokenClient> logger,
    IHttpClientFactory httpClientFactory,
    AgentAuthorizationHeaderProviderService tokenService) : IUserTokenClient
{
    private readonly ILogger<UserTokenClient> _logger = logger;
    private readonly string _apiEndpoint = "https://token.botframework.com";
    private readonly string _scopes = "https://api.botframework.com/.default"; // configuration["AzureAd:AgentScope"]!; // "https://api.botframework.com/.default";
    private readonly JsonSerializerOptions _defaultOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly AgentAuthorizationHeaderProviderService _tokenService = tokenService;

    public AgenticIdentity? AgenticIdentity { get; set; }

    public async Task<IUserTokenClient.GetTokenResult> GetTokenAsync(string userId, string connectionName, string channelId, string? code = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string?>
        {
            { "userid", userId },
            { "connectionName", connectionName },
            { "channelId", channelId }
        };

        if (!string.IsNullOrEmpty(code))
        {
            queryParams.Add("code", code);
        }

        string? resJson = await CallApiAsync("api/usertoken/GetToken", queryParams, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (resJson is not null)
        {
            var result = JsonSerializer.Deserialize<IUserTokenClient.GetTokenResult>(resJson, _defaultOptions)!;
            return result;
        }
        return new IUserTokenClient.GetTokenResult();
    }

    public async Task<IUserTokenClient.GetSignInResourceResult> GetTokenOrSignInResource(string userId, string connectionName, string channelId, string? finalRedirect = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string?>
        {
            { "userid", userId },
            { "connectionName", connectionName },
            { "channelId", channelId }
        };
        var tokenExchangeState = new
        {
            ConnectionName = connectionName,
            Conversation = new
            {
                User = new ConversationAccount { Id = userId },
            }
        };
        var tokenExchangeStateJson = JsonSerializer.Serialize(tokenExchangeState, Activity.DefaultJsonOptions);
        var state = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenExchangeStateJson));

        queryParams.Add("state", state);

        //if (!string.IsNullOrEmpty(finalRedirect))
        //{
        //    queryParams.Add("finalRedirect", finalRedirect);
        //}

        var json = await CallApiAsync("api/usertoken/GetTokenOrSignInResource", queryParams, cancellationToken: cancellationToken).ConfigureAwait(false);
        var result = JsonSerializer.Deserialize<IUserTokenClient.GetSignInResourceResult>(json!, _defaultOptions)!;
        return result;
    }

    public async Task<IUserTokenClient.GetTokenStatusResult> GetTokenStatusAsync(string userId, string channelId, string? include = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string?>
        {
            { "userid", userId },
            { "channelId", channelId }
        };

        if (!string.IsNullOrEmpty(include))
        {
            queryParams.Add("include", include);
        }

        string? json = await CallApiAsync("api/usertoken/GetTokenStatus", queryParams, cancellationToken: cancellationToken).ConfigureAwait(false);
        var result = JsonSerializer.Deserialize<IList<IUserTokenClient.GetTokenStatusResult>>(json!, _defaultOptions)!;
        return result[0]!;

    }

    public async Task<bool> SignOutUserAsync(string userId, string? connectionName = null, string? channelId = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string?>
        {
            { "userid", userId }
        };

        if (!string.IsNullOrEmpty(connectionName))
        {
            queryParams.Add("connectionName", connectionName);
        }

        if (!string.IsNullOrEmpty(channelId))
        {
            queryParams.Add("channelId", channelId);
        }

        try
        {
            await CallApiAsync("api/usertoken/SignOut", queryParams, HttpMethod.Delete, cancellationToken: cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign out user {UserId}", userId);
            return false;
        }
    }

    public Task<string> ExchangeTokenAsync(string userId, string connectionName, string channelId, string exchangeToken, CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string?>
        {
            { "userid", userId },
            { "connectionName", connectionName },
            { "channelId", channelId }
        };

        var body = new
        {
            exchangeable = new
            {
                token = exchangeToken
            }
        };

        return CallApiAsync("api/usertoken/exchange", queryParams, method: HttpMethod.Post, JsonSerializer.Serialize(body), cancellationToken)!;
    }

    public Task<string> GetAadTokensAsync(string userId, string connectionName, string channelId, string[]? resourceUrls = null, CancellationToken cancellationToken = default)
    {
        var body = new
        {
            channelId,
            connectionName,
            userId,
            resourceUrls = resourceUrls ?? []
        };

        return CallApiAsync("api/usertoken/GetAadTokens", body, cancellationToken);
    }

    private async Task<string?> CallApiAsync(string endpoint, Dictionary<string, string?> queryParams, HttpMethod? method = null, string? body = "", CancellationToken cancellationToken = default)
    {
        try
        {
            string token;
            if (AgenticIdentity is null)
            {
                token = await _tokenService.GetAuthorizationHeaderForAppAsync(_scopes, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                token = await _tokenService.GetAuthorizationHeaderAsync(_scopes, AgenticIdentity, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            AuthorizationHeaderProviderOptions options = new()
            {
                AcquireTokenOptions = new AcquireTokenOptions()
                {
                    AuthenticationOptionsName = "AzureAd",
                }
            };

            // Capture the authorization header provider reference at the start of the method
            // to avoid accessing it after potential scope disposal
            var currentAuthProvider = authorizationHeaderProvider ?? throw new ObjectDisposedException(nameof(IAuthorizationHeaderProvider), "Authorization header provider is not available.");
            var authHeader = await currentAuthProvider.CreateAuthorizationHeaderForAppAsync(_scopes, options);
            var httpClient = httpClientFactory.CreateClient("ApiClient");
            string tokenValue = token.StartsWith("Bearer ") ? token["Bearer ".Length..] : token;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);
            var fullPath = $"{_apiEndpoint}/{endpoint}";
            var requestUri = QueryHelpers.AddQueryString(fullPath, queryParams);
            _logger.LogInformation("Calling API endpoint: {Endpoint}", requestUri);

            var httpMethod = method ?? HttpMethod.Get;
            var request = new HttpRequestMessage(httpMethod, requestUri);

            if (httpMethod == HttpMethod.Post && !string.IsNullOrEmpty(body))
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("API call successful. Status: {StatusCode}", response.StatusCode);
                return content;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("User Token not found: {Endpoint}", requestUri);
                    return null!;
                }
                else
                {
                    _logger.LogError("API call failed. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"API call failed with status {response.StatusCode}: {errorContent}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling API");
            throw;
        }
    }

    private async Task<string> CallApiAsync(string endpoint, object body, CancellationToken cancellationToken = default)
    {
        try
        {
            var authHeader = await _tokenService.GetAuthorizationHeaderForAppAsync(_scopes, cancellationToken: cancellationToken).ConfigureAwait(false);
            var httpClient = httpClientFactory.CreateClient("ApiClient");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authHeader);
            var fullPath = $"{_apiEndpoint}/{endpoint}";

            _logger.LogInformation("Calling API endpoint with POST: {Endpoint}", fullPath);

            var jsonContent = JsonSerializer.Serialize(body);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(fullPath, content, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("API call successful. Status: {StatusCode}", response.StatusCode);
                return responseContent;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogError("API call failed. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"API call failed with status {response.StatusCode}: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling API");
            throw;
        }
    }
}