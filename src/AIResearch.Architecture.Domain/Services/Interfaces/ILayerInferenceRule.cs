namespace AIResearch.Architecture.Domain.Services.Interfaces;

/// <summary>
///     Base interface for rules that determine which layer a component type belongs to.
///     Supports both deterministic and ambiguous layer classification.
/// </summary>
public interface ILayerInferenceRule : ILayeringRule
{
    /// <summary>
    ///     The name of the layer this rule applies to.
    /// </summary>
    string LayerName { get; }

    /// <summary>
    ///     The description of the layer this rule applies to.
    /// </summary>
    string LayerDescription { get; }

    /// <summary>
    ///     Determines if the given component type deterministically belongs to this layer.
    /// </summary>
    /// <param name="componentRole">The type of the component (e.g., "Handler", "Entity").</param>
    /// <returns>True if this component type always belongs to this layer.</returns>
    bool AppliesToComponentRole(string componentRole);

    /// <summary>
    ///     Determines if the given component role could potentially belong to this layer.
    /// </summary>
    /// <param name="componentRole">The role of the component (e.g., "Service", "Interface").</param>
    /// <returns>True if this component role could belong to this layer.</returns>
    bool CouldApplyToComponentRole(string componentRole);

    /// <summary>
    ///     Gets the suggested alternative names for this component type in this layer.
    /// </summary>
    /// <param name="componentRole">The role of the component.</param>
    /// <returns>Array of suggested type names.</returns>
    string[] GetSuggestedComponentRoles(string componentRole);
}