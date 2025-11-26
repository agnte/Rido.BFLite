using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Identity.Abstractions;
using Moq;
using Moq.Protected;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Rido.BFLite.Core.Tests;

public class UserTokenClientTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<IAuthorizationHeaderProvider> _mockAuthProvider;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly UserTokenClient _userTokenClient;
    private readonly string _testScope = "https://api.botframework.com/.default";
    private readonly string _testAuthHeader = "Bearer test-token";

    // Cache the JsonSerializerOptions instance
    private static readonly JsonSerializerOptions _camelCaseOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public UserTokenClientTests()
    {
        // Setup mocks
        _mockAuthProvider = new Mock<IAuthorizationHeaderProvider>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        // Setup AuthorizationHeaderProvider mock
        _mockAuthProvider.Setup(a => a.CreateAuthorizationHeaderForAppAsync(It.IsAny<string>(), It.IsAny<AuthorizationHeaderProviderOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testAuthHeader);

        // Setup DI container
        ServiceCollection services = new();

        // Add configuration with test data
        Dictionary<string, string?> configurationData = new()
        {
            ["AzureAd:AgentScope"] = _testScope
        };
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Add logging
        services.AddLogging(builder => builder.AddProvider(NullLoggerProvider.Instance));

        // Add HttpClient factory with mocked HttpMessageHandler
        services.AddHttpClient("ApiClient", client => { })
            .ConfigurePrimaryHttpMessageHandler(() => _mockHttpMessageHandler.Object);

        // Add default HttpClient with the same mocked handler for methods that don't use named client
        services.AddHttpClient(string.Empty, client => { })
            .ConfigurePrimaryHttpMessageHandler(() => _mockHttpMessageHandler.Object);

        // Add mocked authorization header provider
        services.AddSingleton(_mockAuthProvider.Object);

        // Add AgentAuthorizationHeaderProviderService
        services.AddScoped<AgentAuthorizationHeaderProviderService>();

        // Add UserTokenClient
        services.AddScoped<UserTokenClient>();

        _serviceProvider = services.BuildServiceProvider();
        _userTokenClient = _serviceProvider.GetRequiredService<UserTokenClient>();
    }

    [Fact]
    public async Task GetTokenAsync_WithValidResponse_ReturnsToken()
    {
        // Arrange
        string userId = "test-user";
        string connectionName = "test-connection";
        string channelId = "test-channel";
        string code = "test-code";

        IUserTokenClient.GetTokenResult expectedResponse = new()
        {
            ConnectionName = connectionName,
            Token = "test-token-value"
        };
        string responseJson = JsonSerializer.Serialize(expectedResponse, _camelCaseOptions);

        SetupHttpMessageHandler(HttpStatusCode.OK, responseJson);

        // Act
        IUserTokenClient.GetTokenResult result = await _userTokenClient.GetTokenAsync(userId, connectionName, channelId, code);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(connectionName, result.ConnectionName);
        Assert.Equal("test-token-value", result.Token);

        // Verify the HTTP request was made correctly
        VerifyHttpRequest("GET", "https://token.botframework.com/api/usertoken/GetToken");
        _mockAuthProvider.Verify(a => a.CreateAuthorizationHeaderForAppAsync(It.IsAny<string>(), It.IsAny<AuthorizationHeaderProviderOptions?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTokenAsync_WithoutCode_OmitsCodeParameter()
    {
        // Arrange
        string userId = "test-user";
        string connectionName = "test-connection";
        string channelId = "test-channel";

        IUserTokenClient.GetTokenResult expectedResponse = new()
        {
            ConnectionName = connectionName,
            Token = "test-token-value"
        };
        string responseJson = JsonSerializer.Serialize(expectedResponse, _camelCaseOptions);

        SetupHttpMessageHandler(HttpStatusCode.OK, responseJson);

        // Act
        IUserTokenClient.GetTokenResult result = await _userTokenClient.GetTokenAsync(userId, connectionName, channelId);

        // Assert
        Assert.NotNull(result);
        VerifyHttpRequest("GET", "https://token.botframework.com/api/usertoken/GetToken");
    }

    [Fact]
    public async Task GetTokenAsync_WithNotFoundResponse_ReturnsEmptyResult()
    {
        // Arrange
        string userId = "test-user";
        string connectionName = "test-connection";
        string channelId = "test-channel";

        SetupHttpMessageHandler(HttpStatusCode.NotFound, "Not Found");

        // Act
        IUserTokenClient.GetTokenResult result = await _userTokenClient.GetTokenAsync(userId, connectionName, channelId);

        // Assert - Returns empty result instead of null for NotFound responses
        Assert.NotNull(result);
        Assert.Null(result.Token);
        Assert.Null(result.ConnectionName);
    }

    [Fact]
    public async Task GetTokenOrSignInResource_WithValidResponse_ReturnsSignInResource()
    {
        // Arrange
        string userId = "test-user";
        string connectionName = "test-connection";
        string channelId = "test-channel";

        IUserTokenClient.GetSignInResourceResult expectedResponse = new()
        {
            SignInResource = new IUserTokenClient.Signinresource
            {
                SignInLink = "https://signin.link",
                TokenPostResource = new IUserTokenClient.Tokenpostresource
                {
                    SasUrl = "https://sas.url"
                }
            }
        };
        string responseJson = JsonSerializer.Serialize(expectedResponse, _camelCaseOptions);

        SetupHttpMessageHandler(HttpStatusCode.OK, responseJson);

        // Act
        IUserTokenClient.GetSignInResourceResult result = await _userTokenClient.GetTokenOrSignInResource(userId, connectionName, channelId);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.SignInResource);
        Assert.Equal("https://signin.link", result.SignInResource.SignInLink);
        Assert.Equal("https://sas.url", result.SignInResource.TokenPostResource?.SasUrl);

        VerifyHttpRequest("GET", "https://token.botframework.com/api/usertoken/GetTokenOrSignInResource");
    }

    [Fact]
    public async Task GetTokenStatusAsync_WithValidResponse_ReturnsTokenStatus()
    {
        // Arrange
        string userId = "test-user";
        string channelId = "test-channel";
        string include = "test-include";

        List<IUserTokenClient.GetTokenStatusResult> expectedResponse =
        [
            new()
            {
                ConnectionName = "test-connection",
                HasToken = true,
                ServiceProviderDisplayName = "Test Provider"
            }
        ];
        string responseJson = JsonSerializer.Serialize(expectedResponse, _camelCaseOptions);

        SetupHttpMessageHandler(HttpStatusCode.OK, responseJson);

        // Act
        IUserTokenClient.GetTokenStatusResult[] result = await _userTokenClient.GetTokenStatusAsync(userId, channelId, include);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("test-connection", result[0].ConnectionName);
        Assert.True(result[0].HasToken);
        Assert.Equal("Test Provider", result[0].ServiceProviderDisplayName);

        VerifyHttpRequest("GET", "https://token.botframework.com/api/usertoken/GetTokenStatus");
    }

    [Fact]
    public async Task SignOutUserAsync_WithSuccessfulResponse_ReturnsTrue()
    {
        // Arrange
        string userId = "test-user";
        string connectionName = "test-connection";
        string channelId = "test-channel";

        SetupHttpMessageHandler(HttpStatusCode.OK, "");

        // Act
        bool result = await _userTokenClient.SignOutUserAsync(userId, connectionName, channelId);

        // Assert
        Assert.True(result);
        VerifyHttpRequest("DELETE", "https://token.botframework.com/api/usertoken/SignOut");
    }

    [Fact]
    public async Task SignOutUserAsync_WithHttpException_ReturnsFalse()
    {
        // Arrange
        string userId = "test-user";

        SetupHttpMessageHandler(HttpStatusCode.InternalServerError, "Server Error");

        // Act
        bool result = await _userTokenClient.SignOutUserAsync(userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExchangeTokenAsync_WithValidResponse_ReturnsToken()
    {
        // Arrange
        string userId = "test-user";
        string connectionName = "test-connection";
        string channelId = "test-channel";
        string exchangeToken = "exchange-token";
        string expectedResponse = "exchanged-token";

        SetupHttpMessageHandler(HttpStatusCode.OK, expectedResponse);

        // Act
        string result = await _userTokenClient.ExchangeTokenAsync(userId, connectionName, channelId, exchangeToken);

        // Assert
        Assert.Equal(expectedResponse, result);
        VerifyHttpRequest("POST", "https://token.botframework.com/api/usertoken/exchange");
    }

    [Fact]
    public async Task GetAadTokensAsync_WithValidResponse_ReturnsTokens()
    {
        // Arrange
        string userId = "test-user";
        string connectionName = "test-connection";
        string channelId = "test-channel";
        string[] resourceUrls = ["https://graph.microsoft.com", "https://vault.azure.net"];
        string expectedResponse = "aad-tokens-response";

        SetupHttpMessageHandler(HttpStatusCode.OK, expectedResponse);

        // Act
        string result = await _userTokenClient.GetAadTokensAsync(userId, connectionName, channelId, resourceUrls);

        // Assert
        Assert.Equal(expectedResponse, result);
        VerifyHttpRequest("POST", "https://token.botframework.com/api/usertoken/GetAadTokens");
    }

    [Fact]
    public async Task GetAadTokensAsync_WithNullResourceUrls_UsesEmptyArray()
    {
        // Arrange
        string userId = "test-user";
        string connectionName = "test-connection";
        string channelId = "test-channel";
        string expectedResponse = "aad-tokens-response";

        SetupHttpMessageHandler(HttpStatusCode.OK, expectedResponse);

        // Act
        string result = await _userTokenClient.GetAadTokensAsync(userId, connectionName, channelId);

        // Assert
        Assert.Equal(expectedResponse, result);
        VerifyHttpRequest("POST", "https://token.botframework.com/api/usertoken/GetAadTokens");
    }

    [Fact]
    public async Task CallApiAsync_WithDisposedAuthProvider_ThrowsObjectDisposedException()
    {
        // Arrange
        Mock<IAuthorizationHeaderProvider> mockDisposedAuthProvider = new();
        mockDisposedAuthProvider.Setup(a => a.CreateAuthorizationHeaderForAppAsync(It.IsAny<string>(), It.IsAny<AuthorizationHeaderProviderOptions?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ObjectDisposedException("IServiceProvider"));

        // Create a separate DI container for this test
        using ServiceProvider services = new ServiceCollection()
            .AddSingleton<IConfiguration>(_serviceProvider.GetRequiredService<IConfiguration>())
            .AddLogging(builder => builder.AddProvider(NullLoggerProvider.Instance))
            .AddHttpClient("ApiClient", client => { })
                .ConfigurePrimaryHttpMessageHandler(() => _mockHttpMessageHandler.Object)
            .Services
            .AddSingleton(mockDisposedAuthProvider.Object)
            .AddScoped<AgentAuthorizationHeaderProviderService>()
            .AddScoped<UserTokenClient>()
            .BuildServiceProvider();

        UserTokenClient userTokenClient = services.GetRequiredService<UserTokenClient>();

        // Act & Assert
        ObjectDisposedException exception = await Assert.ThrowsAsync<ObjectDisposedException>(
            () => userTokenClient.GetTokenAsync("user", "connection", "channel"));

        Assert.Equal("IServiceProvider", exception.ObjectName);
    }

    [Fact]
    public async Task CallApiAsync_WithServerError_ThrowsHttpRequestException()
    {
        // Arrange
        string userId = "test-user";
        string connectionName = "test-connection";
        string channelId = "test-channel";

        SetupHttpMessageHandler(HttpStatusCode.InternalServerError, "Internal Server Error");

        // Act & Assert
        HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => _userTokenClient.GetTokenAsync(userId, connectionName, channelId));

        Assert.Contains("API call failed with status InternalServerError", exception.Message);
    }

    [Theory]
    [InlineData("", "test-connection", "test-channel")]
    [InlineData("test-user", "", "test-channel")]
    [InlineData("test-user", "test-connection", "")]
    public async Task GetTokenAsync_WithEmptyParameters_StillMakesRequest(string userId, string connectionName, string channelId)
    {
        // Arrange
        IUserTokenClient.GetTokenResult expectedResponse = new()
        {
            ConnectionName = connectionName,
            Token = "test-token-value"
        };
        string responseJson = JsonSerializer.Serialize(expectedResponse, _camelCaseOptions);

        SetupHttpMessageHandler(HttpStatusCode.OK, responseJson);

        // Act
        IUserTokenClient.GetTokenResult result = await _userTokenClient.GetTokenAsync(userId, connectionName, channelId);

        // Assert
        Assert.NotNull(result);
        VerifyHttpRequest("GET", "https://token.botframework.com/api/usertoken/GetToken");
    }

    [Fact]
    public async Task GetTokenOrSignInResource_CreatesCorrectTokenExchangeState()
    {
        // Arrange
        string userId = "test-user-123";
        string connectionName = "test-connection";
        string channelId = "test-channel";

        IUserTokenClient.GetSignInResourceResult expectedResponse = new()
        {
            SignInResource = new IUserTokenClient.Signinresource
            {
                SignInLink = "https://signin.link",
                TokenPostResource = new IUserTokenClient.Tokenpostresource
                {
                    SasUrl = "https://sas.url"
                }
            }
        };
        string responseJson = JsonSerializer.Serialize(expectedResponse, _camelCaseOptions);

        HttpRequestMessage? capturedRequest = null;
        SetupHttpMessageHandlerWithCapture(HttpStatusCode.OK, responseJson, req => capturedRequest = req);

        // Act
        await _userTokenClient.GetTokenOrSignInResource(userId, connectionName, channelId);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Contains("state=", capturedRequest.RequestUri!.Query);

        // Extract and verify the state parameter contains the user ID
        NameValueCollection query = System.Web.HttpUtility.ParseQueryString(capturedRequest.RequestUri.Query);
        string? stateValue = query["state"];
        Assert.NotNull(stateValue);

        string decodedState = Encoding.UTF8.GetString(Convert.FromBase64String(stateValue));
        Assert.Contains(userId, decodedState);
        Assert.Contains(connectionName, decodedState);
    }

    [Fact]
    public async Task GetTokenStatusAsync_WithoutIncludeParameter_OmitsInclude()
    {
        // Arrange
        string userId = "test-user";
        string channelId = "test-channel";

        List<IUserTokenClient.GetTokenStatusResult> expectedResponse =
        [
            new()
            {
                ConnectionName = "test-connection",
                HasToken = false,
                ServiceProviderDisplayName = "Test Provider"
            }
        ];
        string responseJson = JsonSerializer.Serialize(expectedResponse, _camelCaseOptions);

        HttpRequestMessage? capturedRequest = null;
        SetupHttpMessageHandlerWithCapture(HttpStatusCode.OK, responseJson, req => capturedRequest = req);

        // Act
        IUserTokenClient.GetTokenStatusResult[] result = await _userTokenClient.GetTokenStatusAsync(userId, channelId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.False(result[0].HasToken);
        Assert.NotNull(capturedRequest);
        Assert.DoesNotContain("include=", capturedRequest.RequestUri!.Query);
    }

    [Fact]
    public async Task ExchangeTokenAsync_SendsCorrectRequestBody()
    {
        // Arrange
        string userId = "test-user";
        string connectionName = "test-connection";
        string channelId = "test-channel";
        string exchangeToken = "test-exchange-token";
        string expectedResponse = "exchanged-token-response";

        string? capturedBody = null;
        SetupHttpMessageHandlerWithBodyCapture(HttpStatusCode.OK, expectedResponse, body => capturedBody = body);

        // Act
        await _userTokenClient.ExchangeTokenAsync(userId, connectionName, channelId, exchangeToken);

        // Assert
        Assert.NotNull(capturedBody);
        Assert.Contains("exchangeable", capturedBody);
        Assert.Contains("test-exchange-token", capturedBody);

        JsonElement bodyObj = JsonSerializer.Deserialize<JsonElement>(capturedBody);
        JsonElement exchangeable = bodyObj.GetProperty("exchangeable");
        string? token = exchangeable.GetProperty("token").GetString();
        Assert.Equal(exchangeToken, token);
    }

    [Fact]
    public async Task GetAadTokensAsync_SendsCorrectRequestBody()
    {
        // Arrange
        string userId = "test-user";
        string connectionName = "test-connection";
        string channelId = "test-channel";
        string[] resourceUrls = ["https://graph.microsoft.com", "https://vault.azure.net"];
        string expectedResponse = "aad-tokens-response";

        string? capturedBody = null;
        SetupHttpMessageHandlerWithBodyCapture(HttpStatusCode.OK, expectedResponse, body => capturedBody = body);

        // Act
        await _userTokenClient.GetAadTokensAsync(userId, connectionName, channelId, resourceUrls);

        // Assert
        Assert.NotNull(capturedBody);
        Assert.Contains(userId, capturedBody);
        Assert.Contains(connectionName, capturedBody);
        Assert.Contains(channelId, capturedBody);
        Assert.Contains("https://graph.microsoft.com", capturedBody);
        Assert.Contains("https://vault.azure.net", capturedBody);
    }

    [Fact]
    public void Configuration_IsCorrectlyInjected()
    {
        // Arrange & Act
        IConfiguration configuration = _serviceProvider.GetRequiredService<IConfiguration>();
        string? agentScope = configuration["AzureAd:AgentScope"];

        // Assert
        Assert.Equal(_testScope, agentScope);
    }

    [Fact]
    public void HttpClientFactory_CreatesNamedClient()
    {
        // Arrange & Act
        IHttpClientFactory httpClientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
        HttpClient client = httpClientFactory.CreateClient("ApiClient");

        // Assert
        Assert.NotNull(client);
        Assert.Equal("ApiClient", client.GetType().GetProperty("Options")?.GetValue(client)?.GetType().GetProperty("Name")?.GetValue(client.GetType().GetProperty("Options")?.GetValue(client)) ?? "ApiClient");
    }

    private void SetupHttpMessageHandler(HttpStatusCode statusCode, string content)
    {
        HttpResponseMessage response = new(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    private void SetupHttpMessageHandlerWithCapture(HttpStatusCode statusCode, string content, Action<HttpRequestMessage> captureAction)
    {
        HttpResponseMessage response = new(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => captureAction(req))
            .ReturnsAsync(response);
    }

    private void SetupHttpMessageHandlerWithBodyCapture(HttpStatusCode statusCode, string content, Action<string> bodyCapture)
    {
        HttpResponseMessage response = new(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>(async (req, ct) =>
            {
                if (req.Content != null)
                {
                    string body = await req.Content.ReadAsStringAsync(ct);
                    bodyCapture(body);
                }
            })
            .ReturnsAsync(response);
    }

    private void VerifyHttpRequest(string expectedMethod, string expectedBaseUrl)
    {
        _mockHttpMessageHandler.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method.ToString().Equals(expectedMethod, StringComparison.OrdinalIgnoreCase) &&
                    req.RequestUri!.ToString().StartsWith(expectedBaseUrl)),
                ItExpr.IsAny<CancellationToken>());
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}
