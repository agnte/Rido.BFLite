using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Rido.BFLite.Core;

namespace Rido.BFLite.Compat.Adapter;

public class CompatAdapter(BotApplication botApplication, CompatBotAdapter compatBotAdapter) :  IBotFrameworkHttpAdapter
{
    public MiddlewareSet MiddlewareSet { get; } = new MiddlewareSet();

    public Func<ITurnContext, Exception, Task>? OnTurnError { get; set; }

    public CompatAdapter Use(Microsoft.Bot.Builder.IMiddleware middleware)
    {
        MiddlewareSet.Use(middleware);
        return this;
    }

    public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
    {
        botApplication.OnOnActivity = activity =>
        {
            TurnContext turnContext = new(compatBotAdapter, activity.ToCompatActivity());
            turnContext.TurnState.Add<Microsoft.Bot.Connector.Authentication.UserTokenClient>(new CompatUserTokenClient(botApplication.UserTokenClient));
            return bot.OnTurnAsync(turnContext, cancellationToken);
        };
        try
        {
            await botApplication.ProcessAsync(httpRequest.HttpContext, cancellationToken);
        }
        catch (Exception ex)
        {
            if (OnTurnError != null)
            {
                TurnContext turnContext = new(compatBotAdapter, new Activity());
                await OnTurnError(turnContext, ex);
            }
        }
    }

    public async Task ContinueConversationAsync(string botId, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken)
    {
        var turnContext = new TurnContext(compatBotAdapter, reference.GetContinuationActivity());
        await callback(turnContext, cancellationToken);
    }
}
