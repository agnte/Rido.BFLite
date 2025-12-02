using System.Text.Json.Serialization;

namespace Rido.BFLite.Core.Schema;

/// <summary>
/// An attachment in an activity
/// </summary>
public class Attachment
{
    /// <summary>
    /// The content type of the attachment
    /// </summary>
    [JsonPropertyName("contentType")]
    public string? ContentType { get; set; }

    /// <summary>
    /// The content of the attachment
    /// </summary>
    [JsonPropertyName("content")]
    public object? Content { get; set; }

    /// <summary>
    /// The content URL of the attachment
    /// </summary>
    [JsonPropertyName("contentUrl")]
    public string? ContentUrl { get; set; }

    /// <summary>
    /// The name of the attachment
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
