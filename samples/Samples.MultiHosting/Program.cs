using Rido.BFLite.Core.Hosting;

WebApplicationBuilder webAppBuilder = WebApplication.CreateSlimBuilder(args);
webAppBuilder.Services.AddBotAuthenticationEx(["BotIdentity", "AgentIdentity"]);
webAppBuilder.Services.AddBotAuthorizationEx(["BotIdentity", "AgentIdentity"]);

//webAppBuilder.Services.AddBotApplicationClients("BotIdentity");
webAppBuilder.Services.AddBotApplicationClients("AgentIdentity");
webAppBuilder.Services.AddBotApplication<MyBotApplication>();
webAppBuilder.Services.AddBotApplication<MyAgentApplication>();

WebApplication webApp = webAppBuilder.Build();
webApp.UseBotApplication<MyBotApplication>("api/bot/messages");
webApp.UseBotApplication<MyAgentApplication>("api/messages");

webApp.Run();
