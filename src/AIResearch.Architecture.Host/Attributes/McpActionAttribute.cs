namespace AIResearch.Architecture.Host.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class McpActionAttribute(string description) : Attribute
{
    public string? Name { get; set; }
    public string Description { get; } = description;
}