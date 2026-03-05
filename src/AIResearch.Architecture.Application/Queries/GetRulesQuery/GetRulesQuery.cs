using AIResearch.Architecture.Contracts.Mediator;

namespace AIResearch.Architecture.Application.Queries.GetRulesQuery;

public sealed record GetRulesQuery : IRequest<GetRulesQueryResult>;