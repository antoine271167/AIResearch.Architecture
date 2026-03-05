namespace AIResearch.Architecture.Domain.Services.Interfaces;

public interface ILayerDependencyRule : ILayeringRule
{
    bool IsViolated(string layerName, string dependsOn);
}