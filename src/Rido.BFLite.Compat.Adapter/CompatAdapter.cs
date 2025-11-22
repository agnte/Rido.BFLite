using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Rido.BFLite.Core;

namespace Rido.BFLite.Compat.Adapter
{
    public class CompatAdapter(BotApplication botApplication, CompatBotAdapter compatBotAdapter) : IBotFrameworkHttpAdapter
    {
        public Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
        {
            //botApplication.OnMessage = async (activity) =>
            //{
            //    var turnContext = new TurnContext(compatBotAdapter, activity.ToCompatActivity());
            //    await bot.OnTurnAsync(turnContext, cancellationToken);
            //};

            botApplication.OnActivity += async (sender, args) =>
            {
                var turnContext = new TurnContext(compatBotAdapter, args.Activity.ToCompatActivity());
                await bot.OnTurnAsync(turnContext, cancellationToken);
            };

            return botApplication.ProcessAsync(httpRequest.HttpContext);
        }
    }
}
