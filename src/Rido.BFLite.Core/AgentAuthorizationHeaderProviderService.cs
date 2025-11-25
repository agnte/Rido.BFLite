using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace Rido.BFLite.Core;

/// <summary>
/// Service for acquiring authorization headers for Bot Framework API calls.
/// Supports both app-only and agentic (user-delegated) token acquisition.
/// </summary>
public class AgentAuthorizationHeaderProviderService(
    IAuthorizationHeaderProvider authorizationHeaderProvider,
    IConfiguration configuration,
    ILogger<AgentAuthorizationHeaderProviderService> logger)
{
    private readonly IAuthorizationHeaderProvider _authorizationHeaderProvider = authorizationHeaderProvider ?? throw new ArgumentNullException(nameof(authorizationHeaderProvider));
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly ILogger<AgentAuthorizationHeaderProviderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Gets an authorization header for Bot Framework API calls.
    /// Supports both app-only and agentic (user-delegated) token acquisition.
    /// </summary>
    /// <param name="scope">The scope for the token request.</param>
    /// <param name="agenticIdentity">Optional agentic identity for user-delegated token acquisition. If not provided, acquires an app-only token.</param>
    /// <param name="aadConfigSectionName">The configuration section name for Azure AD settings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The authorization header value.</returns>
    public async Task<string> GetAuthorizationHeaderAsync(
        string scope,
        AgenticIdentity? agenticIdentity = null,
        string aadConfigSectionName = "AzureAd",
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(scope);

        AuthorizationHeaderProviderOptions options = new()
        {
            AcquireTokenOptions = new AcquireTokenOptions()
            {
                AuthenticationOptionsName = aadConfigSectionName,
            }
        };

        // Use agentic token if we have a valid identity
        if (agenticIdentity != null && 
            !string.IsNullOrEmpty(agenticIdentity.AgentticAppId) && 
            !string.IsNullOrEmpty(agenticIdentity.AgenticUserId))
        {
            _logger.LogDebug("Acquiring agentic token for appId: {AgenticAppId}, userId: {AgenticUserId}", 
                agenticIdentity.AgentticAppId, agenticIdentity.AgenticUserId);

            options.WithAgentUserIdentity(agenticIdentity.AgentticAppId, Guid.Parse(agenticIdentity.AgenticUserId));
            var token = await authorizationHeaderProvider.CreateAuthorizationHeaderAsync([scope], options, null, cancellationToken).ConfigureAwait(false);
            return token;
        }

        // Fall back to app-only token
        _logger.LogDebug("Acquiring app-only token for scope: {Scope}", scope);
        var appToken = await authorizationHeaderProvider.CreateAuthorizationHeaderForAppAsync(scope, options, cancellationToken).ConfigureAwait(false);
        return appToken;
    }

    /// <summary>
    /// Gets an app-only authorization header for Bot Framework API calls.
    /// This method always acquires an app-only token (no user delegation).
    /// </summary>
    /// <param name="scope">The scope for the token request.</param>
    /// <param name="aadConfigSectionName">The configuration section name for Azure AD settings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The authorization header value.</returns>
    public async Task<string> GetAuthorizationHeaderForAppAsync(
        string scope,
        string aadConfigSectionName = "AzureAd",
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(scope);

        var currentAuthProvider = _authorizationHeaderProvider ??
            throw new ObjectDisposedException(nameof(IAuthorizationHeaderProvider), "Authorization header provider is not available.");

        AuthorizationHeaderProviderOptions options = new()
        {
            AcquireTokenOptions = new AcquireTokenOptions()
            {
                AuthenticationOptionsName = aadConfigSectionName,
            }
        };

        _logger.LogDebug("Acquiring app-only token for scope: {Scope}", scope);

        var token = await currentAuthProvider.CreateAuthorizationHeaderForAppAsync(scope, options, cancellationToken).ConfigureAwait(false);

        return token;
    }
}
