using Rido.BFLite.Core.Hosting;
using Samples.OAuthBot;

WebApplicationBuilder webAppBuilder = WebApplication.CreateSlimBuilder(args);
webAppBuilder.Services.AddBotAuthentication();
webAppBuilder.Services.AddBotAuthorization();
webAppBuilder.Services.AddBotApplication<OAuthBot>();
WebApplication webApp = webAppBuilder.Build();
webApp.UseBotApplication<OAuthBot>();
webApp.Run();

