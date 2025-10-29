namespace Rido.BFLite.Core.Schema;

public class ConversationParameters
{
    [JsonPropertyName("isGroup")]
    public bool? IsGroup { get; set; }

    [JsonPropertyName("bot")]
    public ConversationAccount? Bot { get; set; }

    [JsonPropertyName("members")]
    public List<ConversationAccount>? Members { get; set; }

    [JsonPropertyName("topicName")]
    public string? TopicName { get; set; }

    [JsonPropertyName("activity")]
    public Activity? Activity { get; set; }

    [JsonPropertyName("channelData")]
    public ChannelData? ChannelData { get; set; }

    [JsonPropertyName("tenantId")]
    public string? TenantId { get; set; }

    [JsonExtensionData]
    public ExtendedPropertiesDictionary Properties { get; set; } = [];
}
