using Rido.BFLite.Core.Hosting;
using Rido.BFLite.Core.Schema;
using Rido.BFLite.Teams;

WebApplicationBuilder webAppBuilder = WebApplication.CreateSlimBuilder(args);
webAppBuilder.Services.AddBotAuthentication();
webAppBuilder.Services.AddBotAuthorization();
webAppBuilder.Services.AddBotApplicationClients();
webAppBuilder.Services.AddBotApplication<TeamsBotApplication>();
WebApplication webApp = webAppBuilder.Build();
TeamsBotApplication botApp = webApp.UseBotApplication<TeamsBotApplication>();

botApp.OnMessage = async (activity, cancellationToken) =>
{
    Activity reply = activity.CreateReplyActivity($"you said {activity.Text}, with ❤️ at {DateTime.Now:T}");
    await botApp.SendActivityAsync(reply, cancellationToken);
};

botApp.OnMessageReaction = async (reaction, cancellationToken) =>
{
    string result = @$"Reaction received at {DateTime.Now:T}. " +
    $"                  Added: {reaction.ReactionsAdded?.FirstOrDefault()?.Type} " +
    $"                  Removed: {reaction.ReactionsRemoved?.FirstOrDefault()?.Type}";

    Activity reply = reaction.Activity.CreateReplyActivity(result);
    await botApp.SendActivityAsync(reply, cancellationToken);
};

botApp.OnInstallationUpdate = installationUpdate =>
{
    Console.WriteLine($"Installation update event. Action: {installationUpdate.Action} for {installationUpdate.SelectedChannelId} channel");
};

botApp.OnConversationUpdate = async (conversationUpdate, cancellationToken) =>
{
    string result = " Members changed";
    result += "\n\n Added: \n\n";
    conversationUpdate.MembersAdded?.ToList().ForEach(ma => result += $" **{ma.Name}** \n");
    result += "Removed: \n\n";
    conversationUpdate.MembersRemoved?.ToList().ForEach(mr => result += $" {mr.Name}\n");
    await botApp.SendActivityAsync(conversationUpdate.Activity.CreateReplyActivity(result), cancellationToken);
};

webApp.Run();

