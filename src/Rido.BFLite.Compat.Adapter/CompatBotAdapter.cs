using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Rido.BFLite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rido.BFLite.Compat.Adapter
{
    public class CompatBotAdapter(BotApplication botApplication) : BotAdapter
    {
        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {

            ResourceResponse[] responses = new ResourceResponse[1];
            var a = activities[0].FromCompatActivity();
            string resp = await botApplication.SendActivityAsync(a);
            responses[0] = new ResourceResponse(id: resp);
            return responses;
            //for (int i = 0; i < activities.Length; i++)
            //  {
            //    //responses[i] = await turnContext.SendActivityAsync(activities[i], cancellationToken);
            //    var a = activities[i].FromCompatActivity();

            //    string resp = await botApplication.SendActivityAsync(a);
            //    responses[i] = new ResourceResponse(id: resp);
            //}
            //return responses;
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
