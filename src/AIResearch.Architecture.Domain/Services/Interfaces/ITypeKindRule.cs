using AIResearch.Architecture.Domain.Models.Types;

namespace AIResearch.Architecture.Domain.Services.Interfaces;

/// <summary>
///     Defines a rule for determining the C# type kind based on component role.
/// </summary>
public interface ITypeKindRule : IArchitectureRule
{
    /// <summary>
    ///     Determines if this rule applies to the given component role.
    /// </summary>
    bool AppliesToComponentRole(string componentRole);

    /// <summary>
    ///     Gets the C# type kind for the given component role.
    /// </summary>
    CSharpTypeKind GetTypeKind(string componentRole);
}