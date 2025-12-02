using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using System.Collections.Concurrent;

class EchoBot(ConcurrentDictionary<string, ConversationReference> conversationReferences) : ActivityHandler
{
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {

        if (turnContext.Activity.Text == "fail")
        {
            throw new Exception("Test exception");  
        }

        var convRef = turnContext.Activity.GetConversationReference();
        conversationReferences.AddOrUpdate(convRef.User.Id, convRef, (k, v) => convRef);
        var replyText = $"Echo: {turnContext.Activity.Text}";
        await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);


        var members = await TeamsInfo.GetMembersAsync(turnContext)!;

        await turnContext.SendActivityAsync("members " + string.Join(", ", members.ToList().Select(m => m.Name)));


        UserTokenClient userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
        var tokenResponse = await userTokenClient.GetTokenStatusAsync(turnContext.Activity.From.Id, turnContext.Activity.ChannelId, null, cancellationToken);
        await turnContext.SendActivityAsync(MessageFactory.Text($"Token Status: {string.Join(", ", tokenResponse.Select(t => t.ConnectionName + ": " + (t!.HasToken!.Value ? "Has Token" : "No Token")))}"), cancellationToken);
    }
}