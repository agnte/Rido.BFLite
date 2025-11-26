using Rido.BFLite.Core.Schema;
using System.Collections;

namespace Rido.BFLite.Core;

//public delegate Task ActivityCallBackHandler(CancellationToken cancellationToken = default);

public delegate Task NextDelegate(CancellationToken cancellationToken);
public interface ITurnMiddleWare
{
    Task OnTurnAsync(BotApplication botApplication, Activity activity, NextDelegate next, CancellationToken cancellationToken = default);
}

public  class TurnMiddleware : ITurnMiddleWare, IEnumerable<ITurnMiddleWare>
{

    private readonly IList<ITurnMiddleWare> _middlewares = [];
    public TurnMiddleware Use(ITurnMiddleWare middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }


    public async Task OnTurnAsync(BotApplication botApplication, Activity activity, NextDelegate next, CancellationToken cancellationToken = default)
    {
        await RunPipeline(botApplication, activity, null!, 0, cancellationToken).ConfigureAwait(false);
        await next(cancellationToken).ConfigureAwait(false);
    }

    public Task RunPipeline(BotApplication botApplication, Activity activity, Func<Activity, Task>? callback, int nextMiddlewareIndex, CancellationToken cancellationToken)
    {
        if (nextMiddlewareIndex == _middlewares.Count)
        {
            if (callback is not null)
            {
                return callback!(activity) ?? Task.CompletedTask;
            }
            else
            {
                return Task.CompletedTask;
            }
        }
        var nextMiddleware = _middlewares[nextMiddlewareIndex];
        return nextMiddleware.OnTurnAsync(
            botApplication,
            activity,
            (ct) => RunPipeline(botApplication, activity, callback, nextMiddlewareIndex + 1, ct),
            cancellationToken);

    }

    public IEnumerator<ITurnMiddleWare> GetEnumerator()
    {
        return _middlewares.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _middlewares.GetEnumerator();
    }
}
