using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Application.Queries.GetRulesQuery;

public sealed record GetRulesQueryResult(
    IEnumerable<IArchitectureRule> Rules
);