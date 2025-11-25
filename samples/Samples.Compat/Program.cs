using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Rido.BFLite.Compat.Adapter;
using Rido.BFLite.Core;
using Rido.BFLite.Core.Hosting;
using System.Collections.Concurrent;

var webAppBuilder = WebApplication.CreateBuilder(args);

webAppBuilder.Services.AddBotAuthentication();
webAppBuilder.Services.AddBotAuthorization();
webAppBuilder.Services.AddBotApplicationClients();
webAppBuilder.Services.AddBotApplication<BotApplication>();
webAppBuilder.Services.AddSingleton<CompatBotAdapter>();
webAppBuilder.Services.AddSingleton<IBotFrameworkHttpAdapter, CompatAdapter>();
webAppBuilder.Services.AddSingleton<IBot, EchoBot>();

webAppBuilder.Services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

WebApplication webApp = webAppBuilder.Build();



webApp.MapPost("/api/messages", async (IBotFrameworkHttpAdapter adapter, IBot bot, HttpRequest request, HttpResponse response) =>
    await adapter.ProcessAsync(request, response, bot));

webApp.MapGet("/api/notify", async (HttpRequest request, HttpResponse response) =>
{
    var adapter = webApp.Services.GetRequiredService<IBotFrameworkHttpAdapter>();
    var convRef = webApp.Services.GetRequiredService<ConcurrentDictionary<string, ConversationReference>>().Values.FirstOrDefault();
    await ((CompatAdapter)adapter).ContinueConversationAsync(
        webApp.Configuration["MicrosoftAppId"]!,
        convRef!,
        async (turnContext, cancellationToken) =>
        {
            await turnContext.SendActivityAsync("This is a proactive notification.");
        },
        default);
});

webApp.Run();
