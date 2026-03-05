using AIResearch.Architecture.Domain.Models.Types;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Domain.Services.Rules.TypeKind;

/// <summary>
///     Rule that identifies component roles that should be generated as interfaces.
/// </summary>
internal sealed class InterfaceTypeKindRule(IComponentRoleProvider roleProvider) : ITypeKindRule
{
    public string Id => "TYPE-002";

    public string Description =>
        "Component roles ending with 'Interface' or explicitly defined as interfaces should be generated as C# interfaces.";

    public bool AppliesToComponentRole(string componentRole)
    {
        var metadata = roleProvider.GetRoleMetadata(componentRole);
        return metadata?.TypeKind == CSharpTypeKind.Interface;
    }

    public CSharpTypeKind GetTypeKind(string componentRole) => CSharpTypeKind.Interface;
}