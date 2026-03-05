namespace AIResearch.Architecture.Domain.Models.Structure;

public sealed record ArchitectureLayer(
    string Name,
    string Description,
    IReadOnlyCollection<string> AllowedDependencies
);
