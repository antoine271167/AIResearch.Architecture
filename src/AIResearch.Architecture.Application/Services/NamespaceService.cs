using AIResearch.Architecture.Application.Services.Interfaces;

namespace AIResearch.Architecture.Application.Services;

internal sealed class NamespaceService : INamespaceService
{
    public string BuildNamespace(string solutionName, string layer, string? feature = null) =>
        string.IsNullOrWhiteSpace(feature)
            ? $"{solutionName}.{layer}"
            : $"{solutionName}.{feature}.{layer}";

    public string InferLayerFromTypeName(string typeName)
    {
        // Handle interface prefix (e.g., IOrderRepository -> OrderRepository)
        var nameToAnalyze = typeName;
        if (typeName.Length > 1 && typeName[0] == 'I' && char.IsUpper(typeName[1]))
        {
            nameToAnalyze = typeName[1..];
        }

        // Repository interface -> Application (implementation lives in Infrastructure)
        if (nameToAnalyze.EndsWith("Repository", StringComparison.OrdinalIgnoreCase))
        {
            return "Application";
        }

        // Domain patterns
        if (nameToAnalyze.EndsWith("Entity", StringComparison.OrdinalIgnoreCase) ||
            nameToAnalyze.EndsWith("ValueObject", StringComparison.OrdinalIgnoreCase) ||
            nameToAnalyze.EndsWith("Aggregate", StringComparison.OrdinalIgnoreCase))
        {
            return "Domain";
        }

        // Application patterns
        if (nameToAnalyze.EndsWith("Command", StringComparison.OrdinalIgnoreCase) ||
            nameToAnalyze.EndsWith("Query", StringComparison.OrdinalIgnoreCase) ||
            nameToAnalyze.EndsWith("Handler", StringComparison.OrdinalIgnoreCase) ||
            nameToAnalyze.EndsWith("Service", StringComparison.OrdinalIgnoreCase))
        {
            return "Application";
        }

        // Default to Application layer
        return "Application";
    }

    public string InferNamespaceForDependency(string typeName, string solutionName, string? feature = null)
    {
        var layer = InferLayerFromTypeName(typeName);
        return BuildNamespace(solutionName, layer, feature);
    }
}
