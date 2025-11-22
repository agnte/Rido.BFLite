using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Rido.BFLite.Compat.Adapter;
using Rido.BFLite.Core;
using Rido.BFLite.Core.Hosting;

var webAppBuilder = WebApplication.CreateBuilder(args);

webAppBuilder.Services.AddBotAuthentication();
webAppBuilder.Services.AddBotAuthorization();
webAppBuilder.Services.AddBotApplicationClients();
webAppBuilder.Services.AddBotApplication<BotApplication>();
webAppBuilder.Services.AddSingleton<CompatBotAdapter>();
webAppBuilder.Services.AddSingleton<IBotFrameworkHttpAdapter, CompatAdapter>();
webAppBuilder.Services.AddSingleton<IBot, EchoBot>();
WebApplication webApp = webAppBuilder.Build();

webApp.MapPost("/api/messages", async (IBotFrameworkHttpAdapter adapter, IBot bot, HttpRequest request, HttpResponse response) =>
    await adapter.ProcessAsync(request, response, bot));

webApp.Run();
