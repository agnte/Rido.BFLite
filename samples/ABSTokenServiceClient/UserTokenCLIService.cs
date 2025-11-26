using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rido.BFLite.Core;

namespace ABSTokenServiceClient
{
    internal class UserTokenCLIService(IUserTokenClient userTokenClient, ILogger<UserTokenCLIService> logger) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return ExecuteAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            const string userId = "29:10n4Hk6RsMPuLvAxMNd2zEYU2w1dpvsiLC4QcffJ84rCMp_TKJO_dMzosR4d_K67eAumKyxTzXVYqHQWzRf2ukg";
            const string connectionName = "graph";
            const string channelId = "msteams";

            logger.LogInformation("Application started");

            try
            {
                logger.LogInformation("=== Testing GetTokenStatus ===");
                IUserTokenClient.GetTokenStatusResult[] tokenStatus = await userTokenClient.GetTokenStatusAsync(userId, channelId, null, cancellationToken);
                logger.LogInformation("GetTokenStatus result: {Result}", tokenStatus);

                if (tokenStatus[0].HasToken == true)
                {
                    IUserTokenClient.GetTokenResult tokenResponse = await userTokenClient.GetTokenAsync(userId, connectionName, channelId, null, cancellationToken);
                    logger.LogInformation("GetToken result: {Result}", tokenResponse.Token);
                }
                else
                {
                    IUserTokenClient.GetSignInResourceResult req = await userTokenClient.GetTokenOrSignInResource(userId, connectionName, channelId, null, cancellationToken);
                    logger.LogInformation("GetSignInResource result: {Result}", req.SignInResource!.SignInLink);

                    Console.WriteLine("Code?");
                    string code = Console.ReadLine()!;

                    IUserTokenClient.GetTokenResult tokenResponse2 = await userTokenClient.GetTokenAsync(userId, connectionName, channelId, code, cancellationToken);
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

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during API testing");
            }

            logger.LogInformation("Application completed successfully");
        }
    }
}