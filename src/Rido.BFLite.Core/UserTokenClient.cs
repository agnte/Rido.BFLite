using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Abstractions;
using Rido.BFLite.Core.Schema;
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
    Task<GetTokenResult> GetTokenAsync(string userId, string connectionName, string channelId, string? code = null);

    /// <summary>
    /// Get the raw signin link to be sent to the user for signin for a connection.
    /// </summary>
    Task<GetSignInResourceResult> GetTokenOrSignInResource(string userId, string connectionName, string channelId, string? finalRedirect = null);

    /// <summary>
    /// Gets the token status for each connection for the given user.
    /// </summary>
    Task<GetTokenStatusResult> GetTokenStatusAsync(string userId, string channelId, string? include = null);

    /// <summary>
    /// Signs the user out of a connection.
    /// </summary>
    Task<bool> SignOutUserAsync(string userId, string? connectionName = null, string? channelId = null);

    /// <summary>
    /// Exchanges a token for another token.
    /// </summary>
    Task<string> ExchangeTokenAsync(string userId, string connectionName, string channelId, string exchangeToken);

    /// <summary>
    /// Gets AAD tokens for a user.
    /// </summary>
    Task<string> GetAadTokensAsync(string userId, string connectionName, string channelId, string[]? resourceUrls = null);
}

public class UserTokenClient(
    ILogger<UserTokenClient> logger,
    IHttpClientFactory httpClientFactory,
    IAuthorizationHeaderProvider authorizationHeaderProvider) : IUserTokenClient
{
    private readonly ILogger<UserTokenClient> _logger = logger;
    private readonly string _apiEndpoint = "https://api.botframework.com";
    private readonly string _scopes = "https://api.botframework.com/.default";
    private readonly JsonSerializerOptions _defaultOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<IUserTokenClient.GetTokenResult> GetTokenAsync(string userId, string connectionName, string channelId, string? code = null)
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

        string? resJson = await CallApiAsync("api/usertoken/GetToken", queryParams);
        if (resJson is not null)
        {
            var result = JsonSerializer.Deserialize<IUserTokenClient.GetTokenResult>(resJson, _defaultOptions)!;
            return result;
        }
        return null!;
    }

    public async Task<IUserTokenClient.GetSignInResourceResult> GetTokenOrSignInResource(string userId, string connectionName, string channelId, string? finalRedirect = null)
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

        var json = await CallApiAsync("api/usertoken/GetTokenOrSignInResource", queryParams);
        var result = JsonSerializer.Deserialize<IUserTokenClient.GetSignInResourceResult>(json, _defaultOptions)!;
        return result;
    }

    public async Task<IUserTokenClient.GetTokenStatusResult> GetTokenStatusAsync(string userId, string channelId, string? include = null)
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

        string? json = await CallApiAsync("api/usertoken/GetTokenStatus", queryParams);
        var result = JsonSerializer.Deserialize<IList<IUserTokenClient.GetTokenStatusResult>>(json!, _defaultOptions)!;
        return result[0]!;

    }

    public async Task<bool> SignOutUserAsync(string userId, string? connectionName = null, string? channelId = null)
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
            await CallApiAsync("api/usertoken/SignOut", queryParams, HttpMethod.Delete);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign out user {UserId}", userId);
            return false;
        }
    }

    public Task<string> ExchangeTokenAsync(string userId, string connectionName, string channelId, string exchangeToken)
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

        return CallApiAsync("api/usertoken/exchange", queryParams, method: HttpMethod.Post, JsonSerializer.Serialize(body))!;
    }

    public Task<string> GetAadTokensAsync(string userId, string connectionName, string channelId, string[]? resourceUrls = null)
    {
        var body = new
        {
            channelId,
            connectionName,
            userId,
            resourceUrls = resourceUrls ?? []
        };

        return CallApiAsync("api/usertoken/GetAadTokens", body);
    }

    private async Task<string?> CallApiAsync(string endpoint, Dictionary<string, string?> queryParams, HttpMethod? method = null, string? body = "")
    {
        try
        {
            // Capture the authorization header provider reference at the start of the method
            // to avoid accessing it after potential scope disposal
            var currentAuthProvider = authorizationHeaderProvider;
            if (currentAuthProvider == null)
            {
                throw new ObjectDisposedException(nameof(IAuthorizationHeaderProvider), "Authorization header provider is not available.");
            }

            var authHeader = await currentAuthProvider.CreateAuthorizationHeaderForAppAsync(_scopes);
            var httpClient = httpClientFactory.CreateClient("ApiClient");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authHeader);
            var fullPath = $"{_apiEndpoint}/{endpoint}";
            var requestUri = QueryHelpers.AddQueryString(fullPath, queryParams);
            _logger.LogInformation("Calling API endpoint: {Endpoint}", requestUri);

            var httpMethod = method ?? HttpMethod.Get;
            var request = new HttpRequestMessage(httpMethod, requestUri);

            if (httpMethod == HttpMethod.Post && !string.IsNullOrEmpty(body))
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API call successful. Status: {StatusCode}", response.StatusCode);
                return content;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("User Token not found: {Endpoint}", requestUri);
                    return null!;
                    //throw new HttpRequestException($"API endpoint not found: {requestUri}", null, response.StatusCode);
                }
                else
                {
                    _logger.LogError("API call failed. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"API call failed with status {response.StatusCode}: {errorContent}");
                }
            }
        }
        catch (ObjectDisposedException ex) when (ex.ObjectName == "IServiceProvider")
        {
            _logger.LogError(ex, "Service provider was disposed while calling API endpoint: {Endpoint}. This usually indicates that the HTTP request scope ended before the async operation completed.", endpoint);
            throw new InvalidOperationException("Authentication service is not available. The request scope may have ended before the operation completed.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling API");
            throw;
        }
    }

    private async Task<string> CallApiAsync(string endpoint, object body)
    {
        try
        {
            // Capture the authorization header provider reference at the start of the method
            // to avoid accessing it after potential scope disposal
            var currentAuthProvider = authorizationHeaderProvider;
            if (currentAuthProvider == null)
            {
                throw new ObjectDisposedException(nameof(IAuthorizationHeaderProvider), "Authorization header provider is not available.");
            }

            // Get the authorization header using Microsoft.Identity.Web's built-in provider
            var authHeader = await currentAuthProvider.CreateAuthorizationHeaderForAppAsync(_scopes);

            // Create HttpClient from factory
            var httpClient = httpClientFactory.CreateClient("ApiClient");

            // Add the authorization header
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authHeader);

            // Build the full URI
            var fullPath = $"{_apiEndpoint}/{endpoint}";

            _logger.LogInformation("Calling API endpoint with POST: {Endpoint}", fullPath);

            // Serialize the body to JSON
            var jsonContent = JsonSerializer.Serialize(body);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(fullPath, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API call successful. Status: {StatusCode}", response.StatusCode);
                return responseContent;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API call failed. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"API call failed with status {response.StatusCode}: {errorContent}");
            }
        }
        catch (ObjectDisposedException ex) when (ex.ObjectName == "IServiceProvider")
        {
            _logger.LogError(ex, "Service provider was disposed while calling API endpoint: {Endpoint}. This usually indicates that the HTTP request scope ended before the async operation completed.", endpoint);
            throw new InvalidOperationException("Authentication service is not available. The request scope may have ended before the operation completed.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling API");
            throw;
        }
    }
}
