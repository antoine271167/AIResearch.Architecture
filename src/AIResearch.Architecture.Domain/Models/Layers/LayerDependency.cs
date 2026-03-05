namespace AIResearch.Architecture.Domain.Models.Layers;

public sealed record LayerDependency(
    string LayerName,
    IReadOnlyCollection<string> DependsOn
);
