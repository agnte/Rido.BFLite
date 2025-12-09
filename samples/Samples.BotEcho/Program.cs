using Rido.BFLite.Core.Hosting;
using Rido.BFLite.Core.Schema;
using Rido.BFLite.Teams;
using Rido.BFLite.Teams.Schema;

WebApplicationBuilder webAppBuilder = WebApplication.CreateSlimBuilder(args);
webAppBuilder.Services.AddBotApplication<TeamsBotApplication>();
WebApplication webApp = webAppBuilder.Build();
TeamsBotApplication botApp = webApp.UseBotApplication<TeamsBotApplication>();

TeamsActivity? lastActivity = null;

webApp.MapGet("api/notify", async (HttpContext httpContext) =>
{
    Activity activity = new()
    {
        Type = "message",
        Conversation = new Conversation()
        {
            Id = lastActivity!.Conversation!.Id
        },
        From = lastActivity.Recipient,
        Recipient = lastActivity.From,
        ServiceUrl = lastActivity.ServiceUrl,
        Text = "Proactive"
    };

    await botApp.SendActivityAsync(activity);

    return Results.Ok("Notification endpoint is working");
});

botApp.OnMessage = async (context, cancellationToken) =>
{

    var members = await botApp.ConversationClient.GetConversationMembersAsync(activity.ServiceUrl!, activity.Conversation!.Id!, cancellationToken: cancellationToken);

    Activity reply = activity.CreateReplyActivity($"you said {activity.Text}, with ❤️ at {DateTime.Now:T}");
    lastActivity = reply;
    await botApp.SendActivityAsync(reply, cancellationToken);
};

botApp.OnMessageReaction = async (reaction, context, cancellationToken) =>
{
    string result = @$"Reaction received at {DateTime.Now:T}. " +
    $"                  Added: {reaction.ReactionsAdded?.FirstOrDefault()?.Type} " +
    $"                  Removed: {reaction.ReactionsRemoved?.FirstOrDefault()?.Type}";

    await context.SendActivityAsync(result, cancellationToken);
};

botApp.OnInstallationUpdate = async (installationUpdate, context, cancellationToken) =>
{
    await context.SendActivityAsync($"Installation update event. Action: {installationUpdate.Action} for {installationUpdate.SelectedChannelId} channel", cancellationToken);
};

botApp.OnConversationUpdate = async (conversationUpdate, context, cancellationToken) =>
{
    string result = " Members changed";
    result += "\n\n Added: \n\n";
    conversationUpdate.MembersAdded?.ToList().ForEach(ma => result += $" **{ma.Name}** \n");
    result += "Removed: \n\n";
    conversationUpdate.MembersRemoved?.ToList().ForEach(mr => result += $" {mr.Name}\n");
    await context.SendActivityAsync(result, cancellationToken);
};

webApp.Run();

