using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Rido.BFLite.Core.Schema;
using System.Text.Json;

namespace Rido.BFLite.Core;


public class ActivityEventArgs(Activity activity) : EventArgs
{
    public Activity Activity { get; set; } = activity;
}

public class BotApplication
{
    private readonly ILogger<BotApplication> _logger;
    private ConversationClient? _conversationClient;
    private IUserTokenClient? _userTokenClient;
    private HttpContext? _currentHttpContext;

    public BotApplication()
    {
        _logger = NullLogger<BotApplication>.Instance;
    }

    public BotApplication(IConfiguration config, ILogger<BotApplication> logger)
    {
        _logger = logger;
        logger.LogInformation("Started bot listener on {port} for AppID:{appid}", config["ASPNETCORE_URLS"], config["AzureAd:ClientId"]);
    }

    public event EventHandler<ActivityEventArgs>? OnActivity;

    public Func<Activity, BotApplication, Task>? OnMessage { get; set; }
    public Func<MessageReactionActivityWrapper, BotApplication, Task>? OnMessageReaction { get; set; }
    public Func<ConversationUpdateActivityWrapper, BotApplication, Task>? OnConversationUpdate { get; set; }

    /// <summary>
    /// Gets the UserTokenClient that was resolved for the current request
    /// </summary>
    public IUserTokenClient? UserTokenClient => _userTokenClient;

    /// <summary>
    /// Gets the current HttpContext during request processing
    /// </summary>
    public HttpContext? CurrentHttpContext => _currentHttpContext;

    internal async Task<string> ProcessAsync(HttpContext httpContext)
    {
        try
        {
            // Store the current HttpContext for the duration of request processing
            _currentHttpContext = httpContext;
            
            // Resolve services from the current request scope
            _conversationClient = httpContext.RequestServices.GetService<ConversationClient>() ?? throw new Exception("ConversationClient not registered");
            _userTokenClient = httpContext.RequestServices.GetService<IUserTokenClient>() ?? throw new Exception("UserTokenClient not registered");
            
            Activity activity = await ParseActivityAsync(httpContext.Request.Body) ?? throw new InvalidOperationException("Invalid Activity");
            using (_logger.BeginScope(">> Processing activity {Type} {Id}", activity.Type, activity.Id))
            {
                _logger.LogInformation(">> Received Activity of type {Type}", activity.Type);
                OnActivity?.Invoke(this, new ActivityEventArgs(activity));

                switch (activity.Type)
                {
                    case "message":
                        if (OnMessage != null)
                        {
                            await OnMessage.Invoke(activity, this);
                        }
                        _logger.LogInformation("Message activity handled");
                        break;
                    case "messageReaction": 
                        await OnMessageReaction!.Invoke(new MessageReactionActivityWrapper(activity), this);
                        _logger.LogInformation("MessageReaction activity handled");
                        break;
                    case "conversationUpdate":
                        await OnConversationUpdate!.Invoke(new ConversationUpdateActivityWrapper(activity), this);
                        _logger.LogInformation("ConversationUpdate activity handled");
                        break;
                    default:
                        _logger.LogInformation("Activity {Type} not handled", activity.Type);
                        break;
                }
                return "Processed Activity: " + activity.Type;
            }
        }
        finally
        {
            // Clear all request-scoped references after request processing to prevent access to disposed services
            _conversationClient = null;
            _userTokenClient = null;
            _currentHttpContext = null;
        }
    }

    private async Task<Activity?> ParseActivityAsync(Stream httpContentBody)
    {
        Activity? activity;
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            using StreamReader sr = new(httpContentBody);
            string body = await sr.ReadToEndAsync();
            _logger.LogTrace("Reading activity from request body \n {Body} \n", body);
            activity = Activity.FromJsonString(body);
            File.WriteAllText($"in_act_{activity.Type}_{activity.Id!.Replace("|", "_")}.json", body);
        }
        else
        {
            activity = await JsonSerializer.DeserializeAsync<Activity>(httpContentBody, Activity.DefaultJsonOptions);
        }

        return activity;
    }

    public async Task<IUserTokenClient.GetTokenResult> GetTokenAsync(string userId, string connectionName, string channelId, string? magicCode = null!)
    {
        if (_userTokenClient is null)
        {
            throw new InvalidOperationException("UserTokenClient not available. This method can only be called during active request processing or when the service scope is still valid.");
        }
        return await _userTokenClient.GetTokenAsync(userId, connectionName, channelId, magicCode!);
    }

    public async Task<string> SendActivityAsync(Activity activity)
    {
        if (_conversationClient is null)
        {
            _logger.LogError("ConversationClient not available. This method can only be called during active request processing or when the service scope is still valid.");
            return null!;
        }
        return await _conversationClient!.SendActivityAsync(activity);
    }

    /// <summary>
    /// Gets a service from the current request scope. Only available during active request processing.
    /// </summary>
    /// <typeparam name="T">The type of service to resolve</typeparam>
    /// <returns>The resolved service instance</returns>
    /// <exception cref="InvalidOperationException">Thrown when called outside of active request processing</exception>
    public T? GetService<T>() where T : class
    {
        if (_currentHttpContext is null)
        {
            throw new InvalidOperationException("No active request context. This method can only be called during active request processing.");
        }
        return _currentHttpContext.RequestServices.GetService<T>();
    }

    /// <summary>
    /// Gets a required service from the current request scope. Only available during active request processing.
    /// </summary>
    /// <typeparam name="T">The type of service to resolve</typeparam>
    /// <returns>The resolved service instance</returns>
    /// <exception cref="InvalidOperationException">Thrown when called outside of active request processing or when service is not registered</exception>
    public T GetRequiredService<T>() where T : class
    {
        if (_currentHttpContext is null)
        {
            throw new InvalidOperationException("No active request context. This method can only be called during active request processing.");
        }
        return _currentHttpContext.RequestServices.GetRequiredService<T>();
    }
}
