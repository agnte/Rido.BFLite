using Rido.BFLite.Core;
using Rido.BFLite.Core.Hosting;
using Rido.BFLite.Core.Schema;

WebApplicationBuilder webAppBuilder = WebApplication.CreateSlimBuilder(args);
webAppBuilder.Services.AddBotFrameworkAuthentication();
webAppBuilder.Services.AddBotApplicationClients("BotIdentity");
    //webAppBuilder.Services.AddBotApplicationClients("AgentIdentity");
webAppBuilder.Services.AddBotApplication<MyBotApplication>();
webAppBuilder.Services.AddBotApplication<MyAgentApplication>();

WebApplication webApp = webAppBuilder.Build();
webApp.UseBotApplication<MyBotApplication>("api/bot/messages", "Bot");
webApp.UseBotApplication<MyAgentApplication>("api/messages", "Agent");

webApp.Run();

internal class MyBotApplication : BotApplication
{
    public MyBotApplication() : base() { }
    public MyBotApplication(IConfiguration config, ILogger<BotApplication> logger)
        : base(config, logger)
    {
        OnMessage = async activity =>
        {
            Activity reply = activity.CreateReplyActivity($"you said {activity.Text}, with ❤️ at {DateTime.Now:T}");
            await SendActivityAsync(reply);
        };
    }
}

internal class MyAgentApplication : BotApplication
{
    public MyAgentApplication() : base() { }
    public MyAgentApplication(IConfiguration config, ILogger<BotApplication> logger)
        : base(config, logger)
    {
        OnMessage = async activity =>
        {
            Activity reply = activity.CreateReplyActivity($"Agent received your message: {activity.Text}, at {DateTime.Now:T}");
            await SendActivityAsync(reply);
        };
    }
}