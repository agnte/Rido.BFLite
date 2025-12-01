using Rido.BFLite.Core;
using Rido.BFLite.Core.Hosting;
using Rido.BFLite.Core.Schema;
using Samples.Middleware;

WebApplicationBuilder webAppBuilder = WebApplication.CreateSlimBuilder(args);
webAppBuilder.Services.AddBotApplication<BotApplication>();
WebApplication webApp = webAppBuilder.Build();
BotApplication botApp = webApp.UseBotApplication<BotApplication>();

botApp.Use(new MyTurnMiddleWare());
botApp.Use(new MyTurnMiddleWare());

botApp.OnActivity = async (activity, cancellationToken) =>
{
    Activity reply = activity.CreateReplyActivity($"you said {activity.Text}, with ❤️ at {DateTime.Now:T}");
    await botApp.SendActivityAsync(reply, cancellationToken);
};


webApp.Run();