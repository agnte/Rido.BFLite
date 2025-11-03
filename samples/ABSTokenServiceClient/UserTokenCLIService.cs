using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rido.BFLite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            throw new NotImplementedException();
        }

        protected async  Task ExecuteAsync(CancellationToken stoppingToken)
        {
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
        }
    }
}
