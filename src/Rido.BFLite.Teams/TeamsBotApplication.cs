using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rido.BFLite.Core;
using Rido.BFLite.Teams.Handlers;
using Rido.BFLite.Teams.Schema;

namespace Rido.BFLite.Teams;

public class TeamsBotApplication : BotApplication
{
    public Func<MessageReactionActivityWrapper, CancellationToken, Task>? OnMessageReaction { get; set; }
    public Func<InstallationUpdateWrapper, CancellationToken, Task>? OnInstallationUpdate { get; set; }

    public TeamsBotApplication()
    {
    }

    public TeamsBotApplication(IConfiguration config, ILogger<BotApplication> logger, string serviceKey = "AzureAd") : base(config, logger, serviceKey)
    {
        OnActivity = async activity =>
        {
            logger.LogInformation("New activity received of type {type} from {from}", activity.Type, activity.From?.Id);
            TeamsActivity teamsActivity = TeamsActivity.FromActivity(activity);
            if (teamsActivity.Type == "installationUpdate" && OnInstallationUpdate is not null)
            {
                await OnInstallationUpdate.Invoke(new InstallationUpdateWrapper(teamsActivity), default);
            }
            if (teamsActivity.Type == "messageReaction" && OnMessageReaction is not null)
            {
                await OnMessageReaction.Invoke(new MessageReactionActivityWrapper(teamsActivity), default);
            }
        };
    }
}
