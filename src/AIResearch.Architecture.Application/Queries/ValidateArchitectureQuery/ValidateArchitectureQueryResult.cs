using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Application.Queries.ValidateArchitectureQuery;

public sealed record ValidateArchitectureQueryResult(
    IEnumerable<IArchitectureRule> Rules
);