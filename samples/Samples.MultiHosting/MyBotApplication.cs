using Rido.BFLite.Core;
using Rido.BFLite.Core.Schema;

internal class MyBotApplication : BotApplication
{
    public MyBotApplication() : base() { }
    public MyBotApplication(IConfiguration config, ILogger<BotApplication> logger, string serviceKey)
        : base(config, logger, serviceKey)
    {
        OnMessage = async (activity, cancellationToken) =>
        {
            Activity reply = activity.CreateReplyActivity($"you said {activity.Text}, with ❤️ at {DateTime.Now:T}");
            await SendActivityAsync(reply, cancellationToken);
        };
    }
}
