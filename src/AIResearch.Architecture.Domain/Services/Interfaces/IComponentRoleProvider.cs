using AIResearch.Architecture.Domain.Models.Roles;

namespace AIResearch.Architecture.Domain.Services.Interfaces;

/// <summary>
///     Provides component role definitions and their metadata.
/// </summary>
public interface IComponentRoleProvider
{
    /// <summary>
    ///     Gets all available component roles with their metadata.
    /// </summary>
    IReadOnlyCollection<ComponentRoleMetadata> GetAllRoles();

    /// <summary>
    ///     Gets metadata for a specific component role by name.
    /// </summary>
    /// <param name="roleName">The name of the component role.</param>
    /// <returns>The metadata for the role, or null if not found.</returns>
    ComponentRoleMetadata? GetRoleMetadata(string roleName);

    /// <summary>
    ///     Gets all component roles that match the specified predicate.
    /// </summary>
    IReadOnlyCollection<ComponentRoleMetadata> GetRoles(Func<ComponentRoleMetadata, bool> predicate);
}