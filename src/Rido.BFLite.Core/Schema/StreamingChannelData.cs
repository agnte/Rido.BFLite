using System.Text.Json.Serialization;

namespace Rido.BFLite.Core.Schema;

/// <summary>
/// Channel data for streaming responses
/// </summary>
public class StreamingChannelData : ChannelData
{
    /// <summary>
    /// The ID of the stream
    /// </summary>
    [JsonPropertyName("streamId")]
    public string? StreamId { get; set; }

    /// <summary>
    /// The type of streaming message
    /// </summary>
    [JsonPropertyName("streamType")]
    public string? StreamType { get; set; }

    /// <summary>
    /// The sequence number of the streaming message
    /// </summary>
    [JsonPropertyName("streamSequence")]
    public int? StreamSequence { get; set; }

    /// <summary>
    /// Whether feedback loop is enabled
    /// </summary>
    [JsonPropertyName("feedbackLoopEnabled")]
    public bool? FeedbackLoopEnabled { get; set; }

    /// <summary>
    /// The type of feedback loop
    /// </summary>
    [JsonPropertyName("feedbackLoopType")]
    public string? FeedbackLoopType { get; set; }
}
