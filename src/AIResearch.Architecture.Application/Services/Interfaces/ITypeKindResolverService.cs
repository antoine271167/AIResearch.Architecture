using AIResearch.Architecture.Domain.Models.Types;

namespace AIResearch.Architecture.Application.Services.Interfaces;

/// <summary>
/// Service for resolving type metadata (type kind and accessibility) based on component roles.
/// </summary>
public interface ITypeMetadataResolverService
{
    /// <summary>
    /// Resolves the C# type kind for the given component role.
    /// </summary>
    /// <param name="componentRole">The component role (e.g., "RepositoryInterface", "Command").</param>
    /// <returns>The appropriate C# type kind for code generation.</returns>
    CSharpTypeKind ResolveTypeKind(string componentRole);

    /// <summary>
    /// Resolves the accessibility modifier for the given component role.
    /// </summary>
    /// <param name="componentRole">The component role to check.</param>
    /// <returns>The accessibility modifier: "public" or "internal".</returns>
    string ResolveAccessibility(string componentRole);

    /// <summary>
    /// Determines if the given component role requires an interface implementation.
    /// </summary>
    /// <param name="componentRole">The component role to check.</param>
    /// <returns>True if the role requires an interface implementation; otherwise, false.</returns>
    bool RequiresInterface(string componentRole);
}


