using System.Text.Json;

namespace Rido.BFLite.Core.Schema;

public class ConversationUpdateActivityWrapper(Activity act)
{
    public Activity Activity { get; set; } = act;

    public IList<ConversationAccount>? MembersAdded { get; set; } =
        act.Properties.TryGetValue("membersAdded", out object? value)
            && value is JsonElement je
            && je.ValueKind == JsonValueKind.Array
                ? JsonSerializer.Deserialize<IList<ConversationAccount>>(je.GetRawText())
                : null;

    public IList<ConversationAccount>? MembersRemoved { get; set; } =
        act.Properties.TryGetValue("membersRemoved", out object? value2)
            && value2 is JsonElement je2
            && je2.ValueKind == JsonValueKind.Array
                ? JsonSerializer.Deserialize<IList<ConversationAccount>>(je2.GetRawText())
                : null;
}
