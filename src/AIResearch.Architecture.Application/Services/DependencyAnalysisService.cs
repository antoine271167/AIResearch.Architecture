using AIResearch.Architecture.Application.Services.Interfaces;
using AIResearch.Architecture.Domain.Models.Layers;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Application.Services;

internal sealed class DependencyAnalysisService(IArchitectureLayersProvider layersProvider) : IDependencyAnalysisService
{
    public string DetermineLayer(string namespaceName)
    {
        var layers = layersProvider.GetLayers();

        foreach (var layer in layers)
        {
            if (namespaceName.Contains(layer.Name))
            {
                return layer.Name;
            }
        }

        return "Unknown";
    }

    public string DetermineReferencedLayer(string usingDirective)
    {
        var layers = layersProvider.GetLayers();

        foreach (var layer in layers)
        {
            if (usingDirective.Contains(layer.Name))
            {
                return layer.Name;
            }
        }

        return "Unknown";
    }

    public IEnumerable<LayerDependency> AggregateDependencies(
        Dictionary<string, HashSet<string>> layerDependencies) =>
        layerDependencies
            .Select(kv => new LayerDependency(kv.Key, kv.Value.ToList()));
}