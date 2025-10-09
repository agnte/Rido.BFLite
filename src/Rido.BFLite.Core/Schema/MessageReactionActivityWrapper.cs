using System.Text.Json;

namespace Rido.BFLite.Core.Schema;

public class MessageReactionActivityWrapper(Activity act)
{
    public Activity Activity { get; set; } = act;

    public IList<MessageReaction>? ReactionsAdded { get; set; } =
        act.Properties.TryGetValue("reactionsAdded", out object? value)
            && value is JsonElement je
            && je.ValueKind == JsonValueKind.Array
                ? JsonSerializer.Deserialize<IList<MessageReaction>>(je.GetRawText())
                : null;

    public IList<MessageReaction>? ReactionsRemoved { get; set; } =
        act.Properties.TryGetValue("reactionsRemoved", out object? value2)
            && value2 is JsonElement je2
            && je2.ValueKind == JsonValueKind.Array
                ? JsonSerializer.Deserialize<IList<MessageReaction>>(je2.GetRawText())
                : null;
}

public class MessageReaction
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
