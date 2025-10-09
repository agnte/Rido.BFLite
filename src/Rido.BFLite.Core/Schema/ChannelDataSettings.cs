
namespace Rido.BFLite.Core.Schema;

public class ChannelDataSettings
{
    //[JsonPropertyName("selectedChannel")]
    //public required TeamsChannel SelectedChannel { get; set; }
    [JsonExtensionData]
    public ExtendedPropertiesDictionary Properties { get; set; } = [];
}
