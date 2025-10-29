using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Rido.BFLite.Core.Schema;
using Rido.BFLite.Core;
using System.Text.Json;

namespace Rido.BFLite.Core;


public class ActivityEventArgs(Activity activity) : EventArgs
{
    public Activity Activity { get; set; } = activity;
}

public class BotApplication
{
    private readonly ILogger<BotApplication> _logger;
    private readonly IConfiguration _configuration;
    private ConversationClient? _conversationClient;

    public BotApplication()
    {
        _logger = NullLogger<BotApplication>.Instance;
        _configuration = new ConfigurationBuilder().Build();
    }

    public BotApplication(IConfiguration config, ILogger<BotApplication> logger)
    {
        _logger = logger;
        _configuration = config;
        logger.LogInformation("Started bot listener on {port} for AppID:{appid}", config["ASPNETCORE_URLS"], config["AzureAd:ClientId"]);
    }

    public event EventHandler<ActivityEventArgs>? OnActivity;

    public Func<Activity, Task>? OnMessage { get; set; }
    public Func<MessageReactionActivityWrapper, Task>? OnMessageReaction { get; set; }
    public Func<ConversationUpdateActivityWrapper, Task>? OnConversationUpdate { get; set; }

    

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
                    if (OnMessage is not null)
                    {
                        await OnMessage.Invoke(activity);
                        _logger.LogTrace("Message activity handled");
                    }
                    else
                    {
                        _logger.LogTrace("OnMessage handler is not set.");
                    }
                    break;
                case "messageReaction":
                    if (OnMessageReaction is not null)
                    {
                        await OnMessageReaction.Invoke(new MessageReactionActivityWrapper(activity));
                        _logger.LogTrace("MessageReaction activity handled");
                    }
                    else
                    {
                        _logger.LogTrace("OnMessageReaction handler is not set.");
                    }
                    break;
                case "conversationUpdate":
                    if (OnConversationUpdate is not null)
                    {
                        await OnConversationUpdate.Invoke(new ConversationUpdateActivityWrapper(activity));
                        _logger.LogTrace("ConversationUpdate activity handled");
                    }
                    else
                    {
                        _logger.LogTrace("OnConversationUpdate handler is not set.");
                    }
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

    public async Task<ResourceResponse> SendActivityAsync(Activity activity)
    {
        if (_conversationClient is null)
        {
            throw new Exception("ConversationClient not initialized");
        }
        return await _conversationClient.SendActivityAsync(activity);
    }

    //public async Task<string> CheckConfigAsync()
    //{
    //    var agenticCredentialsProvider = new AgenticCredentialsProvider(_configuration);
    //    var token = await agenticCredentialsProvider.CreateAuthorizationHeaderForAppAsync(_configuration["AzureAd:AgentScope"]!);
    //    _logger.LogTrace("Acquired Token {Token}", token);
    //    return token;
    //}
}
