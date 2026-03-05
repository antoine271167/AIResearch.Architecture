using AIResearch.Architecture.Application.Services.Interfaces;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Application.Services;

internal sealed class LayerDependencyService(IArchitectureLayersProvider architectureLayersProvider)
    : ILayerDependencyService
{
    public string[] GetAllowedDependencies(string layerName)
    {
        var layer = architectureLayersProvider.GetLayers()
            .FirstOrDefault(l => l.Name == layerName);

        return layer?.AllowedDependencies.ToArray() ?? [];
    }

    public bool IsValidDependency(string sourceLayer, string targetLayer)
    {
        var allowedDependencies = GetAllowedDependencies(sourceLayer);
        return allowedDependencies.Contains(targetLayer);
    }
}
