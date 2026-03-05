using AIResearch.Architecture.Contracts.Mediator;

namespace AIResearch.Architecture.Application.Queries.ValidateArchitectureQuery;

public sealed record ValidateArchitectureQuery(
    Dictionary<string, string> Files
) : IRequest<ValidateArchitectureQueryResult>;