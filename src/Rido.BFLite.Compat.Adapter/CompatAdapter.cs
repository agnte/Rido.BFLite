using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Rido.BFLite.Core;

namespace Rido.BFLite.Compat.Adapter;

public class CompatAdapter(BotApplication botApplication, CompatBotAdapter compatBotAdapter) :  IBotFrameworkHttpAdapter
{
    public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
    {
        botApplication.OnOnActivity = activity =>
        {
            TurnContext turnContext = new(compatBotAdapter, activity.ToCompatActivity());
            turnContext.TurnState.Add<Microsoft.Bot.Connector.Authentication.UserTokenClient>(new CompatUserTokenClient(botApplication.UserTokenClient));
            return bot.OnTurnAsync(turnContext, cancellationToken);
        };
        await botApplication.ProcessAsync(httpRequest.HttpContext);
    }

    public async Task ContinueConversationAsync(string botId, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken)
    {
        var turnContext = new TurnContext(compatBotAdapter, reference.GetContinuationActivity());
        await callback(turnContext, cancellationToken);
    }
}
