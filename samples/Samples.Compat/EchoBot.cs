using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

class EchoBot : ActivityHandler
{
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        var replyText = $"Echo: {turnContext.Activity.Text}";
        await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);

        //UserTokenClient userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
        //var tokenResponse = await userTokenClient.GetTokenStatusAsync(turnContext.Activity.From.Id, turnContext.Activity.ChannelId, null, cancellationToken);
        //await turnContext.SendActivityAsync(MessageFactory.Text($"Token Status: {string.Join(", ", tokenResponse.Select(t => t.ConnectionName + ": " + (t.HasToken.Value ? "Has Token" : "No Token")))}"), cancellationToken);
    }
}