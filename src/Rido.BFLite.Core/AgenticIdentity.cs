namespace Rido.BFLite.Core;

internal class AgenticIdentity
{
    public string? AgentticAppId { get; set; }
    public string? AgenticUserId { get; set; }
    public string? AgenticBlueprintId { get; set; }

    public static AgenticIdentity FromProperties(IDictionary<string, object> properties)
    {
        properties.TryGetValue("agenticAppId", out object? appIdObj);
        properties.TryGetValue("agenticUserId", out object? userIdObj);
        properties.TryGetValue("agenticBlueprintId", out object? bluePrintObj);
        return new AgenticIdentity
        {
            AgentticAppId = appIdObj?.ToString(),
            AgenticUserId = userIdObj?.ToString(),
            AgenticBlueprintId = bluePrintObj?.ToString()
        };
    }
}
