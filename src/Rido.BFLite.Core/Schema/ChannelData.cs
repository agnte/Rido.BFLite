namespace Rido.BFLite.Core.Schema;

public class ChannelData()
{
    [JsonPropertyName("clientActivityID")]
    public string? ClientActivityId { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object?> Properties { get; set; } = new Dictionary<string, object?>();
}
