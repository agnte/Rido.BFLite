using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Web;
using Rido.BFLite.Core;
using Rido.BFLite.Core.Schema;


var builder = Host.CreateApplicationBuilder(args);

builder.Services
        .AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd")
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddInMemoryTokenCaches();

builder.Services.AddHttpClient("ApiClient");
builder.Services.AddSingleton<IUserTokenClient, UserTokenClient>();

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var userTokenClient = host.Services.GetRequiredService<IUserTokenClient>();
var config = host.Services.GetRequiredService<IConfiguration>();

// Test data
//const string userId = "c5e99701-2a32-49c1-a660-4629ceeb8c61";
//const string connectionName = "graph";
//const string channelId = "webchat";

const string userId = "29:10n4Hk6RsMPuLvAxMNd2zEYU2w1dpvsiLC4QcffJ84rCMp_TKJO_dMzosR4d_K67eAumKyxTzXVYqHQWzRf2ukg";
const string connectionName = "graph";
const string channelId = "msteams";
Activity activity = new Activity
{
    From = new ConversationAccount { Id = userId },
};

logger.LogInformation("Application started");

try
{
    // 1. Test GetTokenStatus
    logger.LogInformation("=== Testing GetTokenStatus ===");
    var tokenStatus = await userTokenClient.GetTokenStatusAsync(userId, channelId);
    logger.LogInformation("GetTokenStatus result: {Result}", tokenStatus);

    if (tokenStatus.HasToken == true)
    {
        var tokenResponse = await userTokenClient.GetTokenAsync(userId, connectionName, channelId);
        logger.LogInformation("GetToken result: {Result}", tokenResponse.Token);
    }
    else
    {
        var req = await userTokenClient.GetTokenOrSignInResource(userId, connectionName, channelId);
        logger.LogInformation("GetSignInResource result: {Result}", req.SignInResource!.SignInLink);

        Console.WriteLine("Code?");
        string code = Console.ReadLine()!;

        var tokenResponse2 = await userTokenClient.GetTokenAsync(userId, connectionName, channelId, code);
        logger.LogInformation("GetToken With Code result: {Result}", tokenResponse2.Token);
    }


    Console.WriteLine("Want to signout? y/n");
    string yn = Console.ReadLine()!;
    if (yn.ToLowerInvariant() == "y")
    {
        var so = await userTokenClient.SignOutUserAsync(userId, connectionName, channelId);
        logger.LogInformation("SignOutUser result: {Result}", so);
    }
    else
    {
        var tokenResponse = await userTokenClient.GetTokenAsync(userId, connectionName, channelId);
        logger.LogInformation("GetToken result: {Result}", tokenResponse.Token);
    }
   
}
catch (Exception ex)
{
    logger.LogError(ex, "Error during API testing");
}

logger.LogInformation("Application completed successfully");
