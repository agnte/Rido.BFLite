using System.Net.Http.Headers;

namespace Rido.BFLite.Core;

/// <summary>
/// HTTP message handler that automatically acquires and attaches authentication tokens
/// for Bot Framework API calls. Supports both app-only and agentic (user-delegated) token acquisition.
/// </summary>
public class BotAuthenticationHandler : DelegatingHandler
{
    private readonly AgentAuthorizationHeaderProviderService _tokenService;
    private readonly string _scope;
    private readonly string _aadConfigSectionName;

    /// <summary>
    /// Key used to store the agentic identity in HttpRequestMessage options.
    /// </summary>
    public static readonly HttpRequestOptionsKey<AgenticIdentity?> AgenticIdentityKey = new("AgenticIdentity");

    /// <summary>
    /// Initializes a new instance of the <see cref="BotAuthenticationHandler"/> class.
    /// </summary>
    /// <param name="tokenService">The token service for acquiring authorization headers.</param>
    /// <param name="scope">The scope for the token request.</param>
    /// <param name="aadConfigSectionName">The configuration section name for Azure AD settings.</param>
    public BotAuthenticationHandler(
        AgentAuthorizationHeaderProviderService tokenService,
        string scope,
        string aadConfigSectionName = "AzureAd")
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        _aadConfigSectionName = aadConfigSectionName;
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Try to get agentic identity from request options
        // If not present, GetAuthorizationHeaderAsync will fall back to app-only token
        AgenticIdentity? agenticIdentity = null;
        request.Options.TryGetValue(AgenticIdentityKey, out agenticIdentity);

        string token = await _tokenService.GetAuthorizationHeaderAsync(
            _scope,
            agenticIdentity,
            _aadConfigSectionName,
            cancellationToken).ConfigureAwait(false);

        string tokenValue = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? token["Bearer ".Length..]
            : token;

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
