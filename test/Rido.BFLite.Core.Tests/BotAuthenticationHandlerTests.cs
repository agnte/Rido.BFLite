using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Identity.Abstractions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Rido.BFLite.Core.Tests;

public class BotAuthenticationHandlerTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<IAuthorizationHeaderProvider> _mockAuthProvider;
    private readonly Mock<HttpMessageHandler> _mockInnerHandler;
    private readonly string _testScope = "https://api.botframework.com/.default";
    private readonly string _testToken = "test-token-value";
    private readonly string _testAuthHeader = "Bearer test-token-value";

    public BotAuthenticationHandlerTests()
    {
        // Setup mocks
        _mockAuthProvider = new Mock<IAuthorizationHeaderProvider>();
        _mockInnerHandler = new Mock<HttpMessageHandler>();

        // Setup AuthorizationHeaderProvider mock to return token for app-only authentication
        _mockAuthProvider.Setup(a => a.CreateAuthorizationHeaderForAppAsync(It.IsAny<string>(), It.IsAny<AuthorizationHeaderProviderOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testAuthHeader);

        // Setup AuthorizationHeaderProvider mock to return token for agentic authentication
        _mockAuthProvider.Setup(a => a.CreateAuthorizationHeaderAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<AuthorizationHeaderProviderOptions?>(), It.IsAny<ClaimsPrincipal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testAuthHeader);

        // Setup inner handler to return success
        _mockInnerHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });

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

        // Add mocked authorization header provider
        services.AddSingleton(_mockAuthProvider.Object);

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task SendAsync_WithoutAgenticIdentity_AcquiresAppOnlyToken()
    {
        // Arrange
        IAuthorizationHeaderProvider authProvider = _serviceProvider.GetRequiredService<IAuthorizationHeaderProvider>();
        ILogger<BotAuthenticationHandler> logger = _serviceProvider.GetRequiredService<ILogger<BotAuthenticationHandler>>();
        BotAuthenticationHandler handler = new(authProvider, logger, _testScope)
        {
            InnerHandler = _mockInnerHandler.Object
        };
        HttpClient client = new(handler);

        // Act
        HttpResponseMessage response = await client.GetAsync("https://api.example.com/test");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _mockAuthProvider.Verify(a => a.CreateAuthorizationHeaderForAppAsync(_testScope, It.IsAny<AuthorizationHeaderProviderOptions?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_WithAgenticIdentity_AcquiresAgenticToken()
    {
        // Arrange
        AgenticIdentity agenticIdentity = new()
        {
            AgentticAppId = "test-app-id",
            AgenticUserId = Guid.NewGuid().ToString()
        };

        IAuthorizationHeaderProvider authProvider = _serviceProvider.GetRequiredService<IAuthorizationHeaderProvider>();
        ILogger<BotAuthenticationHandler> logger = _serviceProvider.GetRequiredService<ILogger<BotAuthenticationHandler>>();
        BotAuthenticationHandler handler = new(authProvider, logger, _testScope)
        {
            InnerHandler = _mockInnerHandler.Object
        };
        HttpClient client = new(handler);

        HttpRequestMessage request = new(HttpMethod.Get, "https://api.example.com/test");
        request.Options.Set(BotAuthenticationHandler.AgenticIdentityKey, agenticIdentity);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Verify the agentic token was requested (CreateAuthorizationHeaderAsync is called for agentic identity)
        _mockAuthProvider.Verify(a => a.CreateAuthorizationHeaderAsync(
            It.Is<IEnumerable<string>>(scopes => scopes.Contains(_testScope)),
            It.IsAny<AuthorizationHeaderProviderOptions?>(),
            It.IsAny<ClaimsPrincipal?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_SetsAuthorizationHeader()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        _mockInnerHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });

        IAuthorizationHeaderProvider authProvider = _serviceProvider.GetRequiredService<IAuthorizationHeaderProvider>();
        ILogger<BotAuthenticationHandler> logger = _serviceProvider.GetRequiredService<ILogger<BotAuthenticationHandler>>();
        BotAuthenticationHandler handler = new(authProvider, logger, _testScope)
        {
            InnerHandler = _mockInnerHandler.Object
        };
        HttpClient client = new(handler);

        // Act
        await client.GetAsync("https://api.example.com/test");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Headers.Authorization);
        Assert.Equal("Bearer", capturedRequest.Headers.Authorization.Scheme);
        Assert.Equal(_testToken, capturedRequest.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task SendAsync_WithBearerPrefixedToken_StripsBearerPrefix()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        _mockInnerHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });

        IAuthorizationHeaderProvider authProvider = _serviceProvider.GetRequiredService<IAuthorizationHeaderProvider>();
        ILogger<BotAuthenticationHandler> logger = _serviceProvider.GetRequiredService<ILogger<BotAuthenticationHandler>>();
        BotAuthenticationHandler handler = new(authProvider, logger, _testScope)
        {
            InnerHandler = _mockInnerHandler.Object
        };
        HttpClient client = new(handler);

        // Act
        await client.GetAsync("https://api.example.com/test");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Headers.Authorization);
        // The token should not have "Bearer " prefix (should be stripped)
        Assert.Equal(_testToken, capturedRequest.Headers.Authorization.Parameter);
        Assert.False(capturedRequest.Headers.Authorization.Parameter?.StartsWith("Bearer "));
    }

    [Fact]
    public void Constructor_ThrowsOnNullAuthorizationHeaderProvider()
    {
        // Arrange
        ILogger<BotAuthenticationHandler> logger = _serviceProvider.GetRequiredService<ILogger<BotAuthenticationHandler>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BotAuthenticationHandler(null!, logger, _testScope));
    }

    [Fact]
    public void Constructor_ThrowsOnNullScope()
    {
        // Arrange
        IAuthorizationHeaderProvider authProvider = _serviceProvider.GetRequiredService<IAuthorizationHeaderProvider>();
        ILogger<BotAuthenticationHandler> logger = _serviceProvider.GetRequiredService<ILogger<BotAuthenticationHandler>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BotAuthenticationHandler(authProvider, logger, null!));
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}
