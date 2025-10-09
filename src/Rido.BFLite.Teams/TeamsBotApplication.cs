using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rido.BFLite.Core;
using Rido.BFLite.Teams.Schema;

namespace Rido.BFLite.Teams;

public class TeamsBotApplication : BotApplication
{
    public Action<InstallationUpdateWrapper>? OnInstallationUpdate { get; set; }

    public TeamsBotApplication()
    {
    }

    public TeamsBotApplication(IConfiguration config, ILogger<BotApplication> logger) : base(config, logger)
    {
        OnNewActivity += (sender, args) =>
        {
            logger.LogInformation("New activity received of type {type} from {from}", args.Activity.Type, args.Activity.From?.Id);
            TeamsActivity activity = TeamsActivity.FromActivity(args.Activity);
            if (activity.Type == "installationUpdate")
            {
                OnInstallationUpdate?.Invoke(new InstallationUpdateWrapper(activity));
            }
        };
    }
}
