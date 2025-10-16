using Rido.BFLite.Core;
using Rido.BFLite.Core.Hosting;
using Rido.BFLite.Core.Schema;
using Rido.BFLite.Teams;

WebApplicationBuilder webAppBuilder = WebApplication.CreateSlimBuilder(args);
webAppBuilder.Services.AddBotFrameworkAuthentication();
webAppBuilder.Services.AddMessageLoop<TeamsBotApplication>();
WebApplication webApp = webAppBuilder.Build();  
TeamsBotApplication botApp = webApp.UseBotApplication<TeamsBotApplication>();

botApp.OnMessage = async (activity, thisApp) =>
{
    Activity reply = activity.CreateReplyActivity($"you said {activity.Text}, with ❤️ at {DateTime.Now:T}");
    async Task<Activity> ProcessTokenCommandsAsync(Activity activity)
    {
        string text = activity.Text!;
        if (text.StartsWith('/'))
        {
            if (text == "/token")
            {
                IUserTokenClient.GetTokenResult? token = await thisApp.GetTokenAsync(activity.From!.Id!, "graph", activity.ChannelId!);
                if (token is not null)
                {
                    reply.Text = $"Token received: {token.Token}";
                }
                else
                {
                    if (thisApp.UserTokenClient is not null)
                    {
                        IUserTokenClient.GetSignInResourceResult signInResource = await thisApp.UserTokenClient.GetTokenOrSignInResource(activity.From!.Id!, "graph", activity.ChannelId!);
                        reply.Text = $"Please sign in: {signInResource.SignInResource?.SignInLink}";
                    }
                    else
                    {
                        reply.Text = "Authentication service is not available. Please try again.";
                    }
                }
            }
            else if (text == "/logout")
            {
                if (thisApp.UserTokenClient is not null)
                {
                    await thisApp.UserTokenClient.SignOutUserAsync(activity.From!.Id!, "graph", activity.ChannelId);
                    reply.Text = "You have been signed out.";
                }
                else
                {
                    reply.Text = "Authentication service is not available. Please try again.";
                }
            }
            else if (text.StartsWith("/login"))
            {
                if (text.Length > 10)
                {
                    string code = text[7..].Trim();
                    IUserTokenClient.GetTokenResult? token = await thisApp.GetTokenAsync(activity.From!.Id!, "graph", activity.ChannelId!, code);
                    if (token is not null)
                    {
                        reply.Text = $"Token received: {token.Token}";
                    }
                }
                else
                {
                    reply.Text = "Please provide the code received after sign-in. Example: /login <code>";
                }
            }
        }
        return reply;
    }
    reply = await ProcessTokenCommandsAsync(activity);
    await thisApp.SendActivityAsync(reply);
};

botApp.OnMessageReaction = async (reaction, thisApp) =>
{
    string result = @$"Reaction received at {DateTime.Now:T}. " +
    $"                  Added: {reaction.ReactionsAdded?.FirstOrDefault()?.Type} " +
    $"                  Removed: {reaction.ReactionsRemoved?.FirstOrDefault()?.Type}";

    Activity reply = reaction.Activity.CreateReplyActivity(result);
    await thisApp.SendActivityAsync(reply);
};

botApp.OnInstallationUpdate = installationUpdate =>
{
    Console.WriteLine($"Installation update event. Action: {installationUpdate.Action} for {installationUpdate.SelectedChannelId} channel");
};

botApp.OnConversationUpdate = async (conversationUpdate, thisApp) =>
{
    string result = " Members changed";
    result += "\n\n Added: \n\n";
    conversationUpdate.MembersAdded?.ToList().ForEach(ma => result += $" **{ma.Name}** \n");
    result += "Removed: \n\n";
    conversationUpdate.MembersRemoved?.ToList().ForEach(mr => result += $" {mr.Name}\n");

    var tokenStatus = await botApp.UserTokenClient?.GetTokenStatusAsync(conversationUpdate.Activity.From!.Id!, conversationUpdate.Activity.ChannelId!)!;

    result += $"Token available: {tokenStatus.HasToken}";

    await botApp.SendActivityAsync(conversationUpdate.Activity.CreateReplyActivity(result));
};

webApp.Run();

