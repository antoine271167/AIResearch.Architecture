using AIResearch.Architecture.Domain.Models.Layers;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Application.Services.Interfaces;

public interface IDependencyValidationService
{
    IEnumerable<IArchitectureRule> Validate(IEnumerable<LayerDependency> dependencies);
}
