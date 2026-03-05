using AIResearch.Architecture.Domain.Models.Layers;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Domain.Services.Rules.Dependency;

internal class InfrastructureDependencyRule : ILayerDependencyRule
{
    public string Id => "ARCH-003";
    public string Description => "Infrastructure layer may depend on Application and Domain layers.";
    public bool IsViolated(string layerName, string dependsOn) =>
        layerName == nameof(LayerType.Infrastructure)
        && !new[] { nameof(LayerType.Application), nameof(LayerType.Domain) }
            .Contains(dependsOn);
}
