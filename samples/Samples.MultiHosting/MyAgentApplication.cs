using Rido.BFLite.Core;
using Rido.BFLite.Core.Schema;

internal class MyAgentApplication : BotApplication
{
    public MyAgentApplication() : base() { }
    public MyAgentApplication(IConfiguration config, ILogger<BotApplication> logger, string serviceKey)
        : base(config, logger, serviceKey)
    {
        OnMessage = async (activity, cancellationToken) =>
        {
            Activity reply = activity.CreateReplyActivity($"Agent received your message: {activity.Text}, at {DateTime.Now:T}");
            await SendActivityAsync(reply, cancellationToken);
        };
    }
}