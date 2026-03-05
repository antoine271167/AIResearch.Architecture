namespace AIResearch.Architecture.Domain.Services.Interfaces;

/// <summary>
///     Defines a rule for determining whether a component role requires an interface implementation.
/// </summary>
public interface ICodeFeatureRule : IGeneralRule
{
    /// <summary>
    ///     Determines if the given component role requires an interface implementation.
    /// </summary>
    /// <param name="componentRole">The component role to check.</param>
    /// <returns>True if the role requires an interface implementation; otherwise, false.</returns>
    bool IsRequired(string componentRole);
}