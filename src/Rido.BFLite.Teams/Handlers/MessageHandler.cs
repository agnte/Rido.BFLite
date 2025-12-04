using Rido.BFLite.Teams.Schema;

namespace Rido.BFLite.Teams.Handlers
{
    public delegate Task MessageHandler(Context context, CancellationToken cancellationToken = default);
}
