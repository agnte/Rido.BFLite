using System.Text.Json;
using System.Text.Json.Nodes;

namespace Rido.BFLite.Core.Schema;

public class ExtendedPropertiesDictionary : Dictionary<string, object?> { }

public class Activity() : Activity<ChannelData>()
{
    public static new Activity FromJsonString(string json) => JsonSerializer.Deserialize<Activity>(json)!;
}

public class Activity<T>(string type = "message") where T : ChannelData, new()
{
    private readonly static JsonSerializerOptions defaultOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string ToJson() => JsonSerializer.Serialize(this, defaultOptions);

    public static Activity<T> FromJsonString(string json) => JsonSerializer.Deserialize<Activity<T>>(json)!;


    [JsonExtensionData]
    public ExtendedPropertiesDictionary Properties { get; set; } = [];

    public Activity CreateReplyActivity(string text = "")
    {
        Activity result = new()
        {
            Type = "message",
            ChannelId = ChannelId,
            ServiceUrl = ServiceUrl,
            Conversation = Conversation,
            From = Recipient,
            Recipient = From,
            ReplyToId = Id,
            Text = text
        };
        return result!;
    }

    [JsonPropertyName("type")]
    public string Type { get; set; } = type;

    [JsonPropertyName("channelId")]
    public string? ChannelId { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("serviceUrl")]
    public string? ServiceUrl { get; set; }

    [JsonPropertyName("replyToId")]
    public string? ReplyToId { get; set; }

    [JsonPropertyName("channelData")]
    public T? ChannelData { get; set; }

    [JsonPropertyName("from")]
    public ConversationAccount? From { get; set; }

    [JsonPropertyName("recipient")]
    public ConversationAccount? Recipient { get; set; }

    [JsonPropertyName("conversation")]
    public Conversation? Conversation { get; set; }

    [JsonPropertyName("entities")]
    public JsonArray? Entities { get; set; }
}
