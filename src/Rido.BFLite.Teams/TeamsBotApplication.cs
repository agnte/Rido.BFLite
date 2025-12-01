using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rido.BFLite.Core;
using Rido.BFLite.Teams.Handlers;
using Rido.BFLite.Teams.Schema;

namespace Rido.BFLite.Teams;

public class TeamsBotApplication : BotApplication
{
    public MessageReactionHandler? OnMessageReaction { get; set; }
    public InstallationUpdateHandler? OnInstallationUpdate { get; set; }
    public ConversationUpdateHandler? OnConversationUpdate { get; set; }

    public TeamsBotApplication()
    {
    }

    public TeamsBotApplication(IConfiguration config, ILogger<BotApplication> logger, string serviceKey = "AzureAd") 
        : base(config, logger, serviceKey)
    {
        OnActivity = async (activity, cancellationToken) =>
        {
            logger.LogInformation("New activity received of type {type} from {from}", activity.Type, activity.From?.Id);
            TeamsActivity teamsActivity = TeamsActivity.FromActivity(activity);
            if (teamsActivity.Type == "installationUpdate" && OnInstallationUpdate is not null)
            {
                await OnInstallationUpdate.Invoke(new InstallationUpdateArgs(teamsActivity), cancellationToken);
            }
            if (teamsActivity.Type == "messageReaction" && OnMessageReaction is not null)
            {
                await OnMessageReaction.Invoke(new MessageReactionArgs(teamsActivity), cancellationToken);
            }
            if (teamsActivity.Type == "conversationUpdate" && OnConversationUpdate is not null)
            {
                await OnConversationUpdate.Invoke(new ConversationUpdateArgs(teamsActivity), cancellationToken);
            }
        };
    }
}
