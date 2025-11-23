using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Rido.BFLite.Core;

namespace Rido.BFLite.Compat.Adapter;

public class CompatAdapter(BotApplication botApplication, CompatBotAdapter compatBotAdapter) : IBotFrameworkHttpAdapter
{
    public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
    {
        botApplication.OnOnActivity = activity => bot.OnTurnAsync(new TurnContext(compatBotAdapter, activity.ToCompatActivity()), cancellationToken);
        await botApplication.ProcessAsync(httpRequest.HttpContext);
    }
}
