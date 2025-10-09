using Rido.BFLite.Core.Schema;
using System.Text.Json.Serialization;

namespace Rido.BFLite.Teams.Schema;

public class TeamsChannelDataSettings
{
    [JsonPropertyName("selectedChannel")]
    public required TeamsChannel SelectedChannel { get; set; }
    [JsonExtensionData]
    public ExtendedPropertiesDictionary Properties { get; set; } = [];
}
