using Rido.BFLite.Teams.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rido.BFLite.Teams.Handlers;

public delegate Task MessageReactionHandler(MessageReactionArgs reactionActivity, Context context, CancellationToken cancellationToken = default);

public class MessageReactionArgs(TeamsActivity act)
{
    public TeamsActivity Activity { get; set; } = act;

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
    /// <summary>
    /// Known reactions are <see cref="KnownReactions"/>."/>
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class KnownReactions
{
    public const string Like = "like";
    public const string Heart = "heart";
    public const string Laugh = "laugh";
    public const string Surprised = "surprised";
    public const string Sad = "sad";
    public const string Angry = "angry";
}