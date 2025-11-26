using Rido.BFLite.Core;
using Rido.BFLite.Core.Hosting;
using Samples.Middleware;

WebApplicationBuilder webAppBuilder = WebApplication.CreateSlimBuilder(args);
webAppBuilder.Services.AddBotAuthentication();
webAppBuilder.Services.AddBotAuthorization();
webAppBuilder.Services.AddBotApplicationClients();
webAppBuilder.Services.AddBotApplication<BotApplication>();
WebApplication webApp = webAppBuilder.Build();
BotApplication botApp = webApp.UseBotApplication<BotApplication>();

botApp.MiddleWare.Use(new MyTurnMiddleWare());
botApp.MiddleWare.Use(new MyTurnMiddleWare());

botApp.OnMessage = async (activity, cancellationToken) =>
{
    var reply = activity.CreateReplyActivity($"you said {activity.Text}, with ❤️ at {DateTime.Now:T}");
    await botApp.SendActivityAsync(reply, cancellationToken);
};


webApp.Run();