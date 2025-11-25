using ABSTokenServiceClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Rido.BFLite.Core;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddTokenAcquisition(true);
builder.Services.AddInMemoryTokenCaches();
builder.Services.AddHttpClient();
builder.Services.AddAgentIdentities();
builder.Services.AddTransient<IUserTokenClient, UserTokenClient>();
builder.Services.AddHostedService<UserTokenCLIService>();
builder.Services.Configure<MicrosoftIdentityApplicationOptions>("AzureAd", builder.Configuration.GetSection("AzureAd"));
WebApplication host = builder.Build();
host.Run();
