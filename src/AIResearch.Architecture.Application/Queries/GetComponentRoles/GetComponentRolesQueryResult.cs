using AIResearch.Architecture.Domain.Models.Roles;

namespace AIResearch.Architecture.Application.Queries.GetComponentRoles;

public sealed record GetComponentRolesQueryResult(
    IReadOnlyCollection<ComponentRoleMetadata> ComponentRoles
);
