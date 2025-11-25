using Microsoft.IdentityModel.JsonWebTokens;
using Rido.BFLite.Core;
using Rido.BFLite.Core.Schema;

namespace Samples.OAuthBot;

public class OAuthBot : BotApplication
{
    public OAuthBot() { }

    public OAuthBot(IConfiguration config, ILogger<OAuthBot> logger) 
        : base(config, logger)
    {
        base.OnMessage = async (activity, cancellationToken) =>
        {
            if (activity.Text!.StartsWith("/token"))
            {
                IUserTokenClient.GetTokenStatusResult tokenStatus = await base.UserTokenClient.GetTokenStatusAsync(activity.From!.Id!, activity.ChannelId!, cancellationToken: cancellationToken);
                await base.SendActivityAsync(activity.CreateReplyActivity($"Token status: HasToken={tokenStatus.HasToken}, ConnectionName={tokenStatus.ConnectionName}"), cancellationToken);
                if (tokenStatus.HasToken!.Value == true)
                {
                    IUserTokenClient.GetTokenResult token = await base.UserTokenClient.GetTokenAsync(activity.From!.Id!, tokenStatus.ConnectionName!, activity.ChannelId!, cancellationToken: cancellationToken);
                    string res = PrintToken(token);
                    await base.SendActivityAsync(activity.CreateReplyActivity(res), cancellationToken);
                }
                else
                {
                    IUserTokenClient.GetSignInResourceResult signInResource = await base.UserTokenClient.GetTokenOrSignInResource(activity.From!.Id!, tokenStatus.ConnectionName!, activity.ChannelId!, cancellationToken: cancellationToken);
                    await base.SendActivityAsync(activity.CreateReplyActivity($"Please sign in using this link: {signInResource.SignInResource!.SignInLink} and reply with /login `<6 digit code>`"), cancellationToken);
                }
            }
            else if (activity.Text!.StartsWith("/login"))
            {
                IUserTokenClient.GetTokenStatusResult tokenStatus = await base.UserTokenClient.GetTokenStatusAsync(activity.From!.Id!, activity.ChannelId!, cancellationToken: cancellationToken);
                IUserTokenClient.GetTokenResult token = await base.UserTokenClient.GetTokenAsync(activity.From!.Id!, tokenStatus.ConnectionName!, activity.ChannelId!, activity.Text[7..], cancellationToken);
                string res = PrintToken(token);
                await base.SendActivityAsync(activity.CreateReplyActivity(res), cancellationToken);
            }
            else if (activity.Text.StartsWith("/logout"))
            {
                IUserTokenClient.GetTokenStatusResult tokenStatus = await base.UserTokenClient.GetTokenStatusAsync(activity.From!.Id!, activity.ChannelId!, cancellationToken: cancellationToken);
                bool logout = await base.UserTokenClient.SignOutUserAsync(activity.From!.Id!, tokenStatus.ConnectionName, cancellationToken: cancellationToken);
                await base.SendActivityAsync(activity.CreateReplyActivity("logged out"), cancellationToken);
            }
            else
            {
                Activity reply = activity.CreateReplyActivity($"you said {activity.Text}, with ❤️ at {DateTime.Now:T}");
                await base.SendActivityAsync(reply, cancellationToken);
            }
        };
    }
    private static string PrintToken(IUserTokenClient.GetTokenResult token)
    {
        JsonWebToken jwt = new(token.Token);
        string res = "Claims: \n";
        jwt.Claims.ToList().ForEach(c => res += $"{c.Type} : **{c.Value}** \r\n");
        return res;
    }
}
