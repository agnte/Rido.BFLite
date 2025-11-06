using Microsoft.Extensions.Logging.Abstractions;
using Rido.BFLite.Core;
using Rido.BFLite.Core.Hosting;

WebApplicationBuilder webAppBuilder = WebApplication.CreateSlimBuilder(args);
webAppBuilder.Services.AddBotAuthenticationEx(["BotIdentity", "AgentIdentity"]);
webAppBuilder.Services.AddBotAuthorizationEx();

webAppBuilder.Services.AddBotApplicationClients("BotIdentity");
webAppBuilder.Services.AddBotApplicationClients("AgentIdentity");

var botApp = new MyBotApplication(webAppBuilder.Configuration, NullLogger<BotApplication>.Instance, "BotIdentity");
var agentApp = new MyAgentApplication(webAppBuilder.Configuration, NullLogger<BotApplication>.Instance, "AgentIdentity");

webAppBuilder.Services.AddBotApplication(botApp);
webAppBuilder.Services.AddBotApplication(agentApp);

WebApplication webApp = webAppBuilder.Build();
webApp.UseBotApplication(botApp, "api/bot/messages");
webApp.UseBotApplication(agentApp, "api/messages");

webApp.Run();
