using Microsoft.Extensions.Hosting;
using Rido.BFLite.Core.Hosting;
using Rido.BFLite.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Rido.BFLite.Compat.Adapter;

public static class CompatHostingExtensions
{
    public static IHostApplicationBuilder AddCompatAdapter(this IHostApplicationBuilder builder)
    {
        builder.Services.AddBotApplication<BotApplication>();
        builder.Services.AddSingleton<CompatBotAdapter>();
        builder.Services.AddSingleton<IBotFrameworkHttpAdapter, CompatAdapter>();
        return builder;
    }
}
