using Rido.BFLite.Core.Hosting;

WebApplicationBuilder webAppBuilder = WebApplication.CreateSlimBuilder(args);
IConfiguration configuration = webAppBuilder.Configuration;
webAppBuilder.Services
    .AddAuthentication()
    .AddCustomJwtBearer("Bot", "botframework.com", configuration["BotIdentity:ClientId"]!)
    .AddCustomJwtBearer("Agent", configuration["AgentIdentity:TenantId"]!, configuration["AgentIdentity:ClientId"]!);

webAppBuilder.Services.AddAuthorizationBuilder()
    .AddPolicy("BotFrameworkPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add("Bot");
        policy.RequireAuthenticatedUser();
    })
    .AddPolicy("AgenticPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add("Agent");
        policy.RequireAuthenticatedUser();
    });


//webAppBuilder.Services.AddBotApplicationClients("BotIdentity");
webAppBuilder.Services.AddBotApplicationClients("AgentIdentity");
webAppBuilder.Services.AddBotApplication<MyBotApplication>();
webAppBuilder.Services.AddBotApplication<MyAgentApplication>();

WebApplication webApp = webAppBuilder.Build();
webApp.UseBotApplication<MyBotApplication>("api/bot/messages", "BotFrameworkPolicy");
webApp.UseBotApplication<MyAgentApplication>("api/messages", "AgenticPolicy");

webApp.Run();
