namespace AIResearch.Architecture.Application.Services.Interfaces;

/// <summary>
/// Service for building namespaces and project names following the solution naming conventions.
/// </summary>
public interface INamespaceService
{
    /// <summary>
    /// Builds a namespace from solution name, feature, and layer.
    /// </summary>
    /// <param name="solutionName">The solution name (e.g., "IaResearch.OrderService")</param>
    /// <param name="layer">The layer name (e.g., "Domain", "Application", "Infrastructure")</param>
    /// <param name="feature">Optional feature name (e.g., "Order")</param>
    /// <returns>The full namespace (e.g., "IaResearch.OrderService.Order.Domain")</returns>
    string BuildNamespace(string solutionName, string layer, string? feature = null);

    /// <summary>
    /// Infers the layer from a type name based on naming conventions.
    /// </summary>
    /// <param name="typeName">The type name (e.g., "IOrderRepository", "CreateOrderCommand")</param>
    /// <returns>The inferred layer name</returns>
    string InferLayerFromTypeName(string typeName);

    /// <summary>
    /// Infers the full namespace for a dependency type.
    /// </summary>
    /// <param name="typeName">The dependency type name</param>
    /// <param name="solutionName">The solution name</param>
    /// <param name="feature">Optional feature name</param>
    /// <returns>The inferred namespace for the dependency</returns>
    string InferNamespaceForDependency(string typeName, string solutionName, string? feature = null);
}
