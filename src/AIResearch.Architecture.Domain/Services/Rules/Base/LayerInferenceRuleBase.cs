using AIResearch.Architecture.Domain.Models.Layers;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Domain.Services.Rules.Base;

/// <summary>
///     Abstract base class for layer inference rules that provides common functionality
///     for determining which layer a component belongs to.
/// </summary>
internal abstract class LayerInferenceRuleBase(
    IArchitectureLayersProvider layersProvider,
    IComponentRoleProvider roleProvider,
    LayerType layerType)
    : ILayerInferenceRule
{
    public string LayerName => layersProvider.GetLayers()
        .First(l => l.Name == layerType.ToString()).Name;

    public string LayerDescription => layersProvider.GetLayers()
        .First(l => l.Name == layerType.ToString()).Description;

    public bool CouldApplyToComponentRole(string componentRole)
    {
        var metadata = roleProvider.GetRoleMetadata(componentRole);
        if (metadata != null)
        {
            return metadata.Layer == layerType;
        }

        // If not found in provider, check if there are suggested alternatives
        var suggestions = GetSuggestedComponentRoles(componentRole);
        return suggestions.Length > 0;
    }

    public string[] GetSuggestedComponentRoles(string componentRole)
    {
        // Get all roles that match this ambiguous name across different layers
        var allRoles = roleProvider.GetAllRoles();
        var matches = allRoles
            .Where(r => r.Name.Contains(componentRole, StringComparison.OrdinalIgnoreCase) ||
                        r.AlternativeNames.Any(alt => alt.Equals(componentRole, StringComparison.OrdinalIgnoreCase)))
            .Select(r => r.Name)
            .ToArray();

        return matches.Length > 0 ? matches : [];
    }

    public bool AppliesToComponentRole(string componentRole)
    {
        var metadata = roleProvider.GetRoleMetadata(componentRole);
        return metadata?.Layer == layerType;
    }

    public abstract string Id { get; }

    public string Description
    {
        get
        {
            var applicableRoles = roleProvider
                .GetRoles(r => r.Layer == layerType)
                .Select(r => r.Name);
            return "The following component roles belong " +
                   $"to the {layerType} layer: {string.Join(", ", applicableRoles)}.";
        }
    }
}