using AIResearch.Architecture.Application.Services.Interfaces;
using AIResearch.Architecture.Contracts.Mediator;

namespace AIResearch.Architecture.Application.Queries.GetRulesQuery;

internal sealed class GetRulesQueryHandler(
    IArchitectureRulesProvider architectureRulesProvider)
    : IRequestHandler<GetRulesQuery, GetRulesQueryResult>
{
    public Task<GetRulesQueryResult> HandleAsync(GetRulesQuery query,
        CancellationToken cancellationToken)
    {
        var rules = architectureRulesProvider.GetRules();
        var result = new GetRulesQueryResult(rules);
        return Task.FromResult(result);
    }
}