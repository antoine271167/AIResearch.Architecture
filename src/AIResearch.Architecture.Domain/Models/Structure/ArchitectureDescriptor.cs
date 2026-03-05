using AIResearch.Architecture.Contracts.Models;

namespace AIResearch.Architecture.Domain.Models.Structure;

public sealed record ArchitectureDescriptor(
    string Name,
    string Version,
    string Description,
    ArchitectureStyle Style,
    IReadOnlyCollection<ArchitectureLayer> Layers
);
