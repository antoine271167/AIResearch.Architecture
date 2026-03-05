namespace AIResearch.Architecture.Host.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class McpExampleAttribute(
    string description,
    string? inputJson = null,
    string? outputJson = null,
    string? errorMessage = null)
    : Attribute
{
    public string Description { get; } = description;
    public string? InputJson { get; } = inputJson;
    public string? OutputJson { get; } = outputJson;
    public string? ErrorMessage { get; } = errorMessage;
}