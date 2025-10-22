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

    public Action<Activity>? OnMessage { get; set; }
    public Action<MessageReactionActivityWrapper>? OnMessageReaction { get; set; }
    public Action<ConversationUpdateActivityWrapper>? OnConversationUpdate { get; set; }

    internal async Task<string> ProcessAsync(HttpContext httpContext)
    {
        _conversationClient = httpContext.RequestServices.GetService<ConversationClient>() ?? throw new Exception("ConversationClient not registered");
        Activity activity = await ParseActivityAsync(httpContext.Request.Body) ?? throw new InvalidOperationException("Invalid Activity");
        using (_logger.BeginScope("Processing activity {Type} {Id}", activity.Type, activity.Id))
        {
            OnActivity?.Invoke(this, new ActivityEventArgs(activity));

            switch (activity.Type)
            {
                case "message":
                    OnMessage?.Invoke(activity);
                    _logger.LogInformation("Message activity handled");
                    break;
                case "messageReaction":
                    OnMessageReaction?.Invoke(new MessageReactionActivityWrapper(activity));
                    _logger.LogInformation("MessageReaction activity handled");
                    break;
                case "conversationUpdate":
                    OnConversationUpdate?.Invoke(new ConversationUpdateActivityWrapper(activity));
                    _logger.LogInformation("ConversationUpdate activity handled");
                    break;
                default:
                    _logger.LogInformation("Activity {Type} not handled", activity.Type);
                    break;
            }
            return "Processed Activity: " + activity.Type;
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
            //File.WriteAllText($"in_act_{activity.Type}_{activity.Id!.Replace("|", "_")}.json", body);
        }
        else
        {
            activity = await JsonSerializer.DeserializeAsync<Activity>(httpContentBody, Activity.DefaultJsonOptions);
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
