using AIResearch.Architecture.Domain.Models.Types;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Domain.Services.Rules.CodeFeatures;

/// <summary>
///     Rule that determines whether a component should be internal instead of public.
///     If a component is not an interface and requires an interface implementation,
///     then it should be internal (the interface is public, implementation is internal).
/// </summary>
internal sealed class InternalRequiredRule(IComponentRoleProvider roleProvider) : ICodeFeatureRule
{
    public string Id => "CODE-002";

    public string Description =>
        "Determines whether a component should be internal instead of public. " +
        "Components that are not interfaces but require an interface implementation should be internal " +
        "(e.g., OrderRepository class is internal, IOrderRepository interface is public).";

    public bool IsRequired(string componentRole)
    {
        var metadata = roleProvider.GetRoleMetadata(componentRole);
        if (metadata == null)
        {
            return false;
        }

        return
            // If it's an interface, it should be public (not internal)
            metadata.TypeKind != CSharpTypeKind.Interface &&
            // If the role requires an interface, then the implementation should be internal
            metadata.RequiresInterface;
    }
}