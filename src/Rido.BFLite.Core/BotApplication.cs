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
    public Activity? LastActivity { get; private set; }

    public BotApplication()
    {
        _logger = NullLogger<BotApplication>.Instance;
    }

    public BotApplication(IConfiguration config, ILogger<BotApplication> logger)
    {
        _logger = logger;
        logger.LogInformation("Started bot listener on {port} for AppID:{appid}", config["ASPNETCORE_URLS"], config["AzureAd:ClientId"]);
    }

    public event EventHandler<ActivityEventArgs>? OnNewActivity;

    public Action<Activity>? OnMessage { get; set; }
    public Action<MessageReactionActivityWrapper>? OnMessageReaction { get; set; }

    internal async Task<string> ProcessAsync(HttpContext httpContext)
    {
        _conversationClient = httpContext.RequestServices.GetService<ConversationClient>() ?? throw new Exception("ConversationClient not registered");
        _logger.BeginScope("Processing request {method} {path}", httpContext.Request.Method, httpContext.Request.Path);

        Activity? activity = await ParseActivityAsync(httpContext.Request.Body);

        if (activity is null)
        {
            _logger.LogWarning("Invalid activity found in the request");
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsync("Invalid activity");
            return "invalid activity";
        }
        LastActivity = activity;
        OnNewActivity?.Invoke(this, new ActivityEventArgs(activity));

        if (activity.Type == "message")
        {
            OnMessage?.Invoke(activity);
        }
        else if (activity.Type == "messageReaction")
        {
            OnMessageReaction?.Invoke(new MessageReactionActivityWrapper(activity));
        }
        else
        {
            _logger.LogWarning("Unsupported activity type {type} found in the request", activity.Type);
        }
        return "invalid activity";
    }

    private async Task<Activity?> ParseActivityAsync(Stream httpContentBody)
    {
        Activity? activity;
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            using StreamReader sr = new(httpContentBody);
            string body = await sr.ReadToEndAsync();
            _logger.LogTrace("Reading activity from request body \n {Body}", body);
            activity = Activity.FromJsonString(body);
        }
        else
        {
            activity = await JsonSerializer.DeserializeAsync<Activity>(httpContentBody);
        }

        return activity;
    }

    public async Task<string> SendActivityAsync(Activity activity)
    {
        if (_conversationClient is null)
        {
            throw new Exception("ConversationClient not initialized");
        }
        return await _conversationClient.SendActivityAsync(activity);
    }
}
