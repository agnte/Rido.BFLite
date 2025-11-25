using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rido.BFLite.Core;

namespace ABSTokenServiceClient;

internal class UserTokenCLIService(IUserTokenClient userTokenClient, ILogger<UserTokenCLIService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        userTokenClient.AgenticIdentity = new AgenticIdentity()
        {
            AgentticAppId = "f30805e3-3457-4c6e-a0e7-bf0fd623f887",
            AgenticUserId = "715d0396-3a7a-4d44-800d-225d04e4d510",
            AgenticAppBlueprintId = ""
        };
        return ExecuteAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected async  Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //"29:1zEO_D4ExqMVy0N4u4O66aGLq7rKOovkO8X0c3Ww7h50DXLpj2yCsvFA60Ns_fX7LHTQdULeM2xoCkWFeLe1ktQ"
        const string userId = "8:orgid:a5b857ed-abcf-48e8-9d63-43a2955dbe8d";
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
                    IUserTokenClient.GetTokenResult tokenResponse2 = await userTokenClient.GetTokenAsync(userId, connectionName, channelId, code, cancellationToken);
                    logger.LogInformation("GetToken With Code result: {Result}", tokenResponse2.Token);
                }

                IUserTokenClient.GetTokenResult tokenResponse2 = await userTokenClient.GetTokenAsync(userId, connectionName, channelId, code);
                logger.LogInformation("GetToken With Code result: {Result}", tokenResponse2.Token);
            }

                Console.WriteLine("Want to signout? y/n");
                string yn = Console.ReadLine()!;
                if ("y".Equals(yn, StringComparison.OrdinalIgnoreCase))
                {
                    bool so = await userTokenClient.SignOutUserAsync(userId, connectionName, channelId, cancellationToken);
                    logger.LogInformation("SignOutUser result: {Result}", so);
                }
                else
                {
                    IUserTokenClient.GetTokenResult tokenResponse = await userTokenClient.GetTokenAsync(userId, connectionName, channelId, null, cancellationToken);
                    logger.LogInformation("GetToken result: {Result}", tokenResponse.Token);
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
    }
}
