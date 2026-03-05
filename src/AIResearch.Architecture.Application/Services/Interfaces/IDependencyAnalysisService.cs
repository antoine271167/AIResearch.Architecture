using AIResearch.Architecture.Domain.Models.Layers;

namespace AIResearch.Architecture.Application.Services.Interfaces;

internal interface IDependencyAnalysisService
{
    string DetermineLayer(string namespaceName);
    string DetermineReferencedLayer(string usingDirective);
    IEnumerable<LayerDependency> AggregateDependencies(Dictionary<string, HashSet<string>> layerDependencies);
}