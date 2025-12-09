using Rido.BFLite.Core;
using Rido.BFLite.Core.Schema;
using Rido.BFLite.Teams;

internal class MyBotApplication : TeamsBotApplication
{
    public MyBotApplication() : base() { }
    public MyBotApplication(IConfiguration config, ILogger<BotApplication> logger, string serviceKey)
        : base(config, logger, serviceKey)
    {
        OnMessage = async (context, cancellationToken) =>
        {
            await context.SendActivityAsync($"you said {context.Activity.Text}, with ❤️ at {DateTime.Now:T}", cancellationToken);
        };
    }
}
