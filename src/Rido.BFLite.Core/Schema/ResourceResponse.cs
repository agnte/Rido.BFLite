using System.Text.Json.Serialization;

namespace Rido.BFLite.Core.Schema;

/// <summary>
/// Response from sending an activity
/// </summary>
public class ResourceResponse
{
    /// <summary>
    /// The ID of the resource
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}
