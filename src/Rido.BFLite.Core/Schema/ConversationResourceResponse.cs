namespace Rido.BFLite.Core.Schema;

public class ConversationResourceResponse
{
    [JsonPropertyName("activityId")]
    public string? ActivityId { get; set; }

    [JsonPropertyName("serviceUrl")]
    public string? ServiceUrl { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}
