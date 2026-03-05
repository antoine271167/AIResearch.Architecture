using AIResearch.Architecture.Domain.Models.Structure;

namespace AIResearch.Architecture.Domain.Services.Interfaces;

public interface IArchitectureLayersProvider
{
    IReadOnlyCollection<ArchitectureLayer> GetLayers();
}
