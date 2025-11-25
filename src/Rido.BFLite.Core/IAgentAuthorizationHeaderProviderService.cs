using Rido.BFLite.Core.Schema;

namespace Rido.BFLite.Core;

/// <summary>
/// Service for acquiring authorization headers for Bot Framework API calls.
/// Supports both app-only and agentic (user-delegated) token acquisition.
/// </summary>
public interface IAgentAuthorizationHeaderProviderService
{
    /// <summary>
    /// Gets an authorization header for the specified scope.
    /// If activity contains agentic context (agenticAppId and agenticUserId), 
    /// acquires an agentic token; otherwise acquires an app-only token.
    /// </summary>
    /// <param name="scope">The scope to request the token for.</param>
    /// <param name="activity">Optional activity containing agentic context in From.Properties.</param>
    /// <param name="aadConfigSectionName">Configuration section name for AAD settings. Defaults to "AzureAd".</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authorization header value (e.g., "Bearer {token}").</returns>
    Task<string> GetAuthorizationHeaderAsync(
        string scope, 
        Activity? activity = null, 
        string aadConfigSectionName = "AzureAd",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an app-only authorization header for the specified scope.
    /// </summary>
    /// <param name="scope">The scope to request the token for.</param>
    /// <param name="aadConfigSectionName">Configuration section name for AAD settings. Defaults to "AzureAd".</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authorization header value (e.g., "Bearer {token}").</returns>
    Task<string> GetAuthorizationHeaderForAppAsync(
        string scope,
        string aadConfigSectionName = "AzureAd",
        CancellationToken cancellationToken = default);
}
