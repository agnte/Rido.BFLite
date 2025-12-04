using Microsoft.IdentityModel.JsonWebTokens;
using Rido.BFLite.Core;
using Rido.BFLite.Core.Schema;
using Rido.BFLite.Teams;

namespace Samples.OAuthBot;

public class OAuthBot : TeamsBotApplication
{
    public OAuthBot() { }

    public OAuthBot(IConfiguration config, ILogger<OAuthBot> logger) 
        : base(config, logger)
    {
        base.OnMessage = async (context, cancellationToken) =>
        {
            if (context.Activity.Text!.StartsWith("/token"))
            {
                IUserTokenClient.GetTokenStatusResult[] tokenStatus = await base.UserTokenClient.GetTokenStatusAsync(context.Activity.From!.Id!, context.Activity.ChannelId!, cancellationToken: cancellationToken);
                await context.SendActivityAsync($"Token status: HasToken={tokenStatus[0].HasToken}, ConnectionName={tokenStatus[0].ConnectionName}", cancellationToken);
                if (tokenStatus[0].HasToken!.Value == true)
                {
                    IUserTokenClient.GetTokenResult? token = await base.UserTokenClient.GetTokenAsync(context.Activity.From!.Id!, tokenStatus[0].ConnectionName!, context.Activity.ChannelId!, cancellationToken: cancellationToken);
                    string res = PrintToken(token);
                    await context.SendActivityAsync(res, cancellationToken);
                }
                else
                {
                    IUserTokenClient.GetSignInResourceResult signInResource = await base.UserTokenClient.GetTokenOrSignInResource(context.Activity.From!.Id!, tokenStatus[0].ConnectionName!, context.Activity.ChannelId!, cancellationToken: cancellationToken);
                    await context.SendActivityAsync($"Please sign in using this link: {signInResource.SignInResource!.SignInLink} and reply with /login `<6 digit code>`", cancellationToken);
                }
            }
            else if (context.Activity.Text!.StartsWith("/login"))
            {
                IUserTokenClient.GetTokenStatusResult[] tokenStatus = await base.UserTokenClient.GetTokenStatusAsync(context.Activity.From!.Id!, context.Activity.ChannelId!, cancellationToken: cancellationToken);
                IUserTokenClient.GetTokenResult? token = await base.UserTokenClient.GetTokenAsync(context.Activity.From!.Id!, tokenStatus[0].ConnectionName!, context.Activity.ChannelId!, context.Activity.Text[7..], cancellationToken);
                string res = PrintToken(token);
                await context.SendActivityAsync(res, cancellationToken);
            }
            else if (context.Activity.Text.StartsWith("/logout"))
            {
                IUserTokenClient.GetTokenStatusResult[] tokenStatus = await base.UserTokenClient.GetTokenStatusAsync(context.Activity.From!.Id!, context.Activity.ChannelId!, cancellationToken: cancellationToken);
                bool logout = await base.UserTokenClient.SignOutUserAsync(context.Activity.From!.Id!, tokenStatus[0].ConnectionName, cancellationToken: cancellationToken);
                await context.SendActivityAsync("logged out", cancellationToken);
            }
            else
            {
                await context.SendActivityAsync($"you said {context.Activity.Text}, with ❤️ at {DateTime.Now:T}", cancellationToken);
            }
        };
    }
    private static string PrintToken(IUserTokenClient.GetTokenResult? token)
    {
        JsonWebToken jwt = new(token!.Token);
        string res = "Claims: \n";
        jwt.Claims.ToList().ForEach(c => res += $"{c.Type} : **{c.Value}** \r\n");
        return res;
    }
}
