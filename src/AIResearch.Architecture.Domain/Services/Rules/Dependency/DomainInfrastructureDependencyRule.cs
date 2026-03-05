using AIResearch.Architecture.Domain.Models.Layers;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Domain.Services.Rules.Dependency;

internal class DomainInfrastructureDependencyRule : ILayerDependencyRule
{
    public string Id => "ARCH-002";
    public string Description => "Domain layer must not depend on Infrastructure layer.";
    public bool IsViolated(string layerName, string dependsOn) =>
        layerName == nameof(LayerType.Domain) && dependsOn == nameof(LayerType.Infrastructure);
}
