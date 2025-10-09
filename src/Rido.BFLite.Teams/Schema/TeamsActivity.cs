using Rido.BFLite.Core.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rido.BFLite.Teams.Schema;

public class TeamsActivity : Activity<TeamsChannelData>
{
    public static TeamsActivity FromActivity(Activity activity) => new(activity);
    public static new TeamsActivity FromJsonString(string json) => new(Activity.FromJsonString(json));

    private TeamsActivity(Activity activity)
    {
        Id = activity.Id;
        ServiceUrl = activity.ServiceUrl;
        ChannelId = activity.ChannelId;
        Type = activity.Type;
        ReplyToId = activity.ReplyToId;
        Text = activity.Text;
        ChannelData = new TeamsChannelData(activity.ChannelData!);
        From = new TeamsConversationAccount(activity.From!);
        Recipient = new TeamsConversationAccount(activity.Recipient!);
        Conversation = new TeamsConversation(activity.Conversation!);
    }


    [JsonPropertyName("from")] public new TeamsConversationAccount From { get; set; }
    [JsonPropertyName("recipient")] public new TeamsConversationAccount Recipient { get; set; }
    [JsonPropertyName("conversation")] public new TeamsConversation Conversation { get; set; }
    [JsonPropertyName("channelData")] public new TeamsChannelData? ChannelData { get; set; }
}
