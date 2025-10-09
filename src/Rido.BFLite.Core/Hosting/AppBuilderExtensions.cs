using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
namespace Rido.BFLite.Core.Hosting;

public static class AppBuilderExtensions
{
    public static T UseBotApplication<T>(this IApplicationBuilder builder) where T : BotApplication, new()
    {
        WebApplication? webApp = builder as WebApplication;
        T app = builder.ApplicationServices.GetService<T>() ?? throw new Exception("Application not registered");
        builder.UseAuthentication();
        builder.UseAuthorization();

        webApp?.MapPost("api/messages", async (HttpContext httpContext) =>
        {
            string resp = await app.ProcessAsync(httpContext);
            return resp;
        }).RequireAuthorization("Bot");

        return app;
    }
}
