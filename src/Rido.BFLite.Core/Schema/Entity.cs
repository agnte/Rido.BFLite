using System.Text.Json.Serialization;

namespace Rido.BFLite.Core.Schema;

/// <summary>
/// An entity in an activity
/// </summary>
public class Entity
{
    /// <summary>
    /// The type of the entity
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Additional properties of the entity
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object?>? Properties { get; set; }

    /// <summary>
    /// Creates a new entity with the specified type
    /// </summary>
    /// <param name="type">The entity type</param>
    public Entity(string type)
    {
        Type = type;
        Properties = new Dictionary<string, object?>();
    }

    /// <summary>
    /// Creates a new entity
    /// </summary>
    public Entity()
    {
        Properties = new Dictionary<string, object?>();
    }
}
