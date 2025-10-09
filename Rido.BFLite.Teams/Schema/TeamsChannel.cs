using System.Text.Json.Serialization;

namespace Rido.BFLite.Teams.Schema;

public class TeamsChannel
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("aadObjectId")]
    public string? AadObjectId { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
