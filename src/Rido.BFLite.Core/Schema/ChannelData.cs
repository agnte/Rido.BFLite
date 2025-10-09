namespace Rido.BFLite.Core.Schema;

public class ChannelData()
{
    [JsonPropertyName("clientActivityId")]
    public string? ClientActivityId { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object?> Properties { get; set; } = new Dictionary<string, object?>();
}
