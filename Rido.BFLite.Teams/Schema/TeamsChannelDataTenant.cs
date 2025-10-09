using System.Text.Json.Serialization;

namespace Rido.BFLite.Teams.Schema
{
    public class TeamsChannelDataTenant
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }
}
