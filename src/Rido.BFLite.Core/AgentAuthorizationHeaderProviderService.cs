using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Rido.BFLite.Core.Schema;

namespace Rido.BFLite.Core;

/// <summary>
/// Service for acquiring authorization headers for Bot Framework API calls.
/// Supports both app-only and agentic (user-delegated) token acquisition.
/// </summary>
public class AgentAuthorizationHeaderProviderService(
    IAuthorizationHeaderProvider authorizationHeaderProvider,
    IConfiguration configuration,
    ILogger<AgentAuthorizationHeaderProviderService> logger) : IAgentAuthorizationHeaderProviderService
{
    private readonly IAuthorizationHeaderProvider _authorizationHeaderProvider = authorizationHeaderProvider ?? throw new ArgumentNullException(nameof(authorizationHeaderProvider));
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly ILogger<AgentAuthorizationHeaderProviderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc/>
    public async Task<string> GetAuthorizationHeaderAsync(
        string scope, 
        Activity? activity = null, 
        string aadConfigSectionName = "AzureAd",
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(scope);

        try
        {
            var currentAuthProvider = _authorizationHeaderProvider ?? 
                throw new ObjectDisposedException(nameof(IAuthorizationHeaderProvider), "Authorization header provider is not available.");

            AuthorizationHeaderProviderOptions options = new()
            {
                AcquireTokenOptions = new AcquireTokenOptions()
                {
                    AuthenticationOptionsName = aadConfigSectionName,
                }
            };

            string token;

            if (activity != null && TryGetAgenticContext(activity, out var agenticAppId, out var agenticUserId))
            {
                _logger.LogDebug("Acquiring agentic token for appId: {AgenticAppId}, userId: {AgenticUserId}", agenticAppId, agenticUserId);
                
                options.WithAgentUserIdentity(agenticAppId, Guid.Parse(agenticUserId));
                token = await currentAuthProvider.CreateAuthorizationHeaderAsync([scope], options, null, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                _logger.LogDebug("Acquiring app-only token for scope: {Scope}", scope);
                token = await currentAuthProvider.CreateAuthorizationHeaderForAppAsync(scope, options, cancellationToken).ConfigureAwait(false);
            }

            return token;
        }
        catch (ObjectDisposedException ex) when (ex.ObjectName == "IServiceProvider")
        {
            _logger.LogError(ex, "Service provider was disposed while acquiring authorization header. This usually indicates that the HTTP request scope ended before the async operation completed.");
            throw new InvalidOperationException("Authentication service is not available. The request scope may have ended before the operation completed.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring authorization header for scope: {Scope}", scope);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string> GetAuthorizationHeaderForAppAsync(
        string scope,
        string aadConfigSectionName = "AzureAd",
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(scope);

        try
        {
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
        catch (ObjectDisposedException ex) when (ex.ObjectName == "IServiceProvider")
        {
            _logger.LogError(ex, "Service provider was disposed while acquiring authorization header. This usually indicates that the HTTP request scope ended before the async operation completed.");
            throw new InvalidOperationException("Authentication service is not available. The request scope may have ended before the operation completed.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring app-only authorization header for scope: {Scope}", scope);
            throw;
        }
    }

    private bool TryGetAgenticContext(Activity activity, out string agenticAppId, out string agenticUserId)
    {
        agenticAppId = string.Empty;
        agenticUserId = string.Empty;

        if (activity.From?.Properties == null)
        {
            return false;
        }

        var hasAgenticAppId = activity.From.Properties.TryGetValue("agenticAppId", out object? agenticAppIdObj);
        var hasAgenticUserId = activity.From.Properties.TryGetValue("agenticUserId", out object? agenticUserIdObj);

        if (!hasAgenticAppId || !hasAgenticUserId || agenticAppIdObj == null || agenticUserIdObj == null)
        {
            return false;
        }

        agenticAppId = agenticAppIdObj.ToString()!;
        agenticUserId = agenticUserIdObj.ToString()!;

        return !string.IsNullOrEmpty(agenticAppId) && !string.IsNullOrEmpty(agenticUserId);
    }
}
