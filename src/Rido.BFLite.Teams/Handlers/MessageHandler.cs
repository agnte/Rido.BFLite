using Rido.BFLite.Teams.Schema;

namespace Rido.BFLite.Teams.Handlers
{
    public delegate Task MessageHandler(TeamsActivity activity, CancellationToken cancellationToken = default);
}
