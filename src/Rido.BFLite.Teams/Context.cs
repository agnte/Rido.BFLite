using Rido.BFLite.Core.Schema;
using Rido.BFLite.Teams.Schema;

namespace Rido.BFLite.Teams;

public class Context(TeamsBotApplication botApplication, TeamsActivity activity)
{
    public TeamsBotApplication BotApplication { get;  } = botApplication;
    public TeamsActivity Activity { get; } = activity;

    public async Task<string> SendActivityAsync(string text, CancellationToken cancellationToken = default)
    {
        Activity reply = Activity.CreateReplyActivity(text);
        return await BotApplication.SendActivityAsync(reply, cancellationToken);
    }
}
