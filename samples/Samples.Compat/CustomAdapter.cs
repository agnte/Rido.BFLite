using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Rido.BFLite.Compat.Adapter;
using Rido.BFLite.Core;

namespace Samples.Compat;

public class CustomAdapter : CompatAdapter
{
    public CustomAdapter(BotApplication botApplication, CompatBotAdapter compatBotAdapter, ILogger<CustomAdapter> logger) 
        : base(botApplication, compatBotAdapter)
    {
        base.Use(new MyMiddleware(logger));

        base.OnTurnError = async (turnContext, exception) =>
        {
            logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");
            await turnContext.SendActivityAsync("The bot encountered an error or bug.");
            await turnContext.SendActivityAsync("To continue to run this bot, please fix the bot source code.");
        };
    }
}
