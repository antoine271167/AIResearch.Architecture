using AIResearch.Architecture.Domain.Models.Types;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Domain.Services.Rules.TypeKind;

/// <summary>
///     Default rule that identifies component roles that should be generated as classes.
///     This is the fallback rule when no other type kind rule applies.
/// </summary>
internal sealed class ClassTypeKindRule(IComponentRoleProvider roleProvider) : ITypeKindRule
{
    public string Id => "TYPE-001";

    public string Description => "Services, handlers, repositories, and entities should be generated as C# classes.";

    public bool AppliesToComponentRole(string componentRole)
    {
        var metadata = roleProvider.GetRoleMetadata(componentRole);
        return metadata?.TypeKind == CSharpTypeKind.Class;
    }

    public CSharpTypeKind GetTypeKind(string componentRole) => CSharpTypeKind.Class;
}