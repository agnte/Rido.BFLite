namespace Rido.BFLite.Core.Schema;

public class PagedMembersResult
{
    [JsonPropertyName("continuationToken")]
    public string? ContinuationToken { get; set; }

    [JsonPropertyName("members")]
    public List<ConversationAccount>? Members { get; set; }
}
