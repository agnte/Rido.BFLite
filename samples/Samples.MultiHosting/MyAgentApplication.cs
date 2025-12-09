using Rido.BFLite.Core;
using Rido.BFLite.Core.Schema;
using Rido.BFLite.Teams;

internal class MyAgentApplication : TeamsBotApplication
{
    public MyAgentApplication() : base() { }
    public MyAgentApplication(IConfiguration config, ILogger<BotApplication> logger, string serviceKey)
        : base(config, logger, serviceKey)
    {
        OnMessage = async (context, cancellationToken) =>
        {
            await context.SendActivityAsync($"Agent received your message: {context.Activity.Text}, at {DateTime.Now:T}", cancellationToken);
        };
    }
}