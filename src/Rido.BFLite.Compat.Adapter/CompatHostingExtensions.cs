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
        builder.Services.AddCompatAdapter();
        return builder;
    }

    public static IServiceCollection AddCompatAdapter(this IServiceCollection services)
    {
        services.AddBotApplication<BotApplication>();
        services.AddSingleton<CompatBotAdapter>();
        services.AddSingleton<IBotFrameworkHttpAdapter, CompatAdapter>();
        return services;
    }
}
