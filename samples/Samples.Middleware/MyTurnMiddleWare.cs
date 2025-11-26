using Rido.BFLite.Core;
using Rido.BFLite.Core.Schema;

namespace Samples.Middleware;

public class MyTurnMiddleWare : ITurnMiddleWare
{
    public Task OnTurnAsync(BotApplication botApplication, Activity activity, NextDelegate next, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Processing activity {activity.Type} {activity.Id}");
        return next(cancellationToken);
    }
}
