using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Domain.Services.Rules.CodeFeatures;

/// <summary>
///     Rule that determines whether a component role requires an interface implementation.
///     Uses the metadata from ComponentRoleProvider to make this determination.
/// </summary>
internal sealed class InterfaceRequiredRule(IComponentRoleProvider roleProvider) : ICodeFeatureRule
{
    public string Id => "CODE-001";

    public string Description =>
        "Determines whether a component role requires an interface implementation (e.g., Repository implements IRepository).";

    public bool IsRequired(string componentRole)
    {
        var metadata = roleProvider.GetRoleMetadata(componentRole);
        return metadata?.RequiresInterface ?? false;
    }
}
