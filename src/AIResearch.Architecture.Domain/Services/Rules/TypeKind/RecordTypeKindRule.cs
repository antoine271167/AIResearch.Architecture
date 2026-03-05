using AIResearch.Architecture.Domain.Models.Types;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Domain.Services.Rules.TypeKind;

/// <summary>
///     Rule that identifies component roles that should be generated as records.
/// </summary>
internal sealed class RecordTypeKindRule(IComponentRoleProvider roleProvider) : ITypeKindRule
{
    public string Id => "TYPE-003";

    public string Description =>
        "Immutable data structures like ValueObjects, Commands, Queries, and Events should be generated as C# records.";

    public bool AppliesToComponentRole(string componentRole)
    {
        var metadata = roleProvider.GetRoleMetadata(componentRole);
        return metadata?.TypeKind == CSharpTypeKind.Record;
    }

    public CSharpTypeKind GetTypeKind(string componentRole) => CSharpTypeKind.Record;
}