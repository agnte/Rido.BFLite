using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Rido.BFLite.Core;


HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services
        .AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd")
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddInMemoryTokenCaches();

builder.Services.AddHttpClient("ApiClient");
builder.Services.AddSingleton<IUserTokenClient, UserTokenClient>();

IHost host = builder.Build();

ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();
IUserTokenClient userTokenClient = host.Services.GetRequiredService<IUserTokenClient>();
IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

// Test data
//const string userId = "c5e99701-2a32-49c1-a660-4629ceeb8c61";
//const string connectionName = "graph";
//const string channelId = "webchat";

const string userId = "29:10n4Hk6RsMPuLvAxMNd2zEYU2w1dpvsiLC4QcffJ84rCMp_TKJO_dMzosR4d_K67eAumKyxTzXVYqHQWzRf2ukg";
const string connectionName = "graph";
const string channelId = "msteams";

logger.LogInformation("Application started");

try
{
    logger.LogInformation("=== Testing GetTokenStatus ===");
    IUserTokenClient.GetTokenStatusResult tokenStatus = await userTokenClient.GetTokenStatusAsync(userId, channelId);
    logger.LogInformation("GetTokenStatus result: {Result}", tokenStatus);

    if (tokenStatus.HasToken == true)
    {
        IUserTokenClient.GetTokenResult tokenResponse = await userTokenClient.GetTokenAsync(userId, connectionName, channelId);
        logger.LogInformation("GetToken result: {Result}", tokenResponse.Token);
    }
    else
    {
        IUserTokenClient.GetSignInResourceResult req = await userTokenClient.GetTokenOrSignInResource(userId, connectionName, channelId);
        logger.LogInformation("GetSignInResource result: {Result}", req.SignInResource!.SignInLink);

        Console.WriteLine("Code?");
        string code = Console.ReadLine()!;

        IUserTokenClient.GetTokenResult tokenResponse2 = await userTokenClient.GetTokenAsync(userId, connectionName, channelId, code);
        logger.LogInformation("GetToken With Code result: {Result}", tokenResponse2.Token);
    }


    Console.WriteLine("Want to signout? y/n");
    string yn = Console.ReadLine()!;
    if (yn.ToLowerInvariant() == "y")
    {
        bool so = await userTokenClient.SignOutUserAsync(userId, connectionName, channelId);
        logger.LogInformation("SignOutUser result: {Result}", so);
    }
    else
    {
        IUserTokenClient.GetTokenResult tokenResponse = await userTokenClient.GetTokenAsync(userId, connectionName, channelId);
        logger.LogInformation("GetToken result: {Result}", tokenResponse.Token);
    }

}
catch (Exception ex)
{
    logger.LogError(ex, "Error during API testing");
}

logger.LogInformation("Application completed successfully");
