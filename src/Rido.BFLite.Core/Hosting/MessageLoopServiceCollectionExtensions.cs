using Microsoft.Extensions.DependencyInjection;

namespace Rido.BFLite.Core.Hosting;

public static class MessageLoopServiceCollectionExtensions
{
    public static IServiceCollection AddMessageLoop<T>(this IServiceCollection services) where T : BotApplication, new()
    {
        services.AddScoped<IUserTokenClient, UserTokenClient>();
        services.AddScoped<ConversationClient>();
        services.AddSingleton<T>();
        return services;
    }
}
