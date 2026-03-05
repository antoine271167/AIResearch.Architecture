using AIResearch.Architecture.Contracts.Mediator;
using AIResearch.Architecture.Contracts.Models;
using AIResearch.Architecture.Domain.Models.Structure;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Application.Queries.GetArchitecture;

internal sealed class GetArchitectureQueryHandler(
    IArchitectureLayersProvider architectureLayersProvider)
    : IRequestHandler<GetArchitectureQuery, GetArchitectureQueryResult>
{
    public Task<GetArchitectureQueryResult> HandleAsync(GetArchitectureQuery request,
        CancellationToken cancellationToken)
    {
        var architecture = new ArchitectureDescriptor(
            "IaResearch .NET Backend Architecture",
            "1.0",
            "Layered architecture with strict dependency rules for .NET services.",
            ArchitectureStyle.Layered,
            architectureLayersProvider.GetLayers()
        );

        var result = new GetArchitectureQueryResult(architecture);

        return Task.FromResult(result);
    }
}