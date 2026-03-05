using AIResearch.Architecture.Domain.Models.Layers;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Domain.Services.Rules.Dependency;

internal class ApplicationDomainDependencyRule : ILayerDependencyRule
{
    public string Id => "ARCH-001";
    public string Description => "Application layer may only depend on Domain layer.";

    public bool IsViolated(string layerName, string dependsOn) =>
        layerName == nameof(LayerType.Application) && dependsOn != nameof(LayerType.Domain);
}