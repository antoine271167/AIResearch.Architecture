using AIResearch.Architecture.Contracts.Mediator;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Application.Queries.GetComponentRoles;

internal sealed class GetComponentRolesQueryHandler(
    IComponentRoleProvider componentRoleProvider)
    : IRequestHandler<GetComponentRolesQuery, GetComponentRolesQueryResult>
{
    public Task<GetComponentRolesQueryResult> HandleAsync(GetComponentRolesQuery query,
        CancellationToken cancellationToken)
    {
        var roles = componentRoleProvider.GetAllRoles();
        var result = new GetComponentRolesQueryResult(roles);
        return Task.FromResult(result);
    }
}
