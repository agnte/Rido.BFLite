using Rido.BFLite.Core;
using Rido.BFLite.Core.Hosting;
using Rido.BFLite.Core.Schema;
using Rido.BFLite.Teams;

WebApplicationBuilder webAppBuilder = WebApplication.CreateSlimBuilder(args);
webAppBuilder.Services.AddBotFrameworkAuthentication();
webAppBuilder.Services.AddMessageLoop<TeamsBotApplication>();
WebApplication webApp = webAppBuilder.Build();
TeamsBotApplication botApp = webApp.UseBotApplication<TeamsBotApplication>();

botApp.OnMessage = async activity =>
{
    Activity reply = activity.CreateReplyActivity($"you said {activity.Text}, with ❤️ at {DateTime.Now:T}");
    await botApp.SendActivityAsync(reply);
};

botApp.OnMessageReaction = async reaction =>
{
    string result = @$"Reaction received at {DateTime.Now:T}. " +
    $"                  Added: {reaction.ReactionsAdded?.FirstOrDefault()?.Type} " +
    $"                  Removed: {reaction.ReactionsRemoved?.FirstOrDefault()?.Type}";

    Activity reply = reaction.Activity.CreateReplyActivity(result);
    await botApp.SendActivityAsync(reply);
};

botApp.OnInstallationUpdate = installationUpdate =>
{
    Console.WriteLine($"Installation update event. Action: {installationUpdate.Action} for {installationUpdate.SelectedChannelId} channel");
};

botApp.OnConversationUpdate = async conversationUpdate =>
{
    string result = $@" Members have changed \n ";
    result += "Added: \n";
    conversationUpdate.MembersAdded?.ToList().ForEach(ma => result += $" {ma.Id} - {ma.Name} \n");
    result += "Removed: \n";
    conversationUpdate.MembersRemoved?.ToList().ForEach(mr => result += $" {mr.Id} - {mr.Name}\n");
    await botApp.SendActivityAsync(conversationUpdate.Activity.CreateReplyActivity(result));
};

webApp.Run();

