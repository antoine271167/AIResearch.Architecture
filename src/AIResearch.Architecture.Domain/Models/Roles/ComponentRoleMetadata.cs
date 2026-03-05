using AIResearch.Architecture.Domain.Models.Layers;
using AIResearch.Architecture.Domain.Models.Types;

namespace AIResearch.Architecture.Domain.Models.Roles;

/// <summary>
///     Metadata describing a component role and its characteristics.
/// </summary>
public sealed record ComponentRoleMetadata(
    string Name,
    CSharpTypeKind TypeKind,
    LayerType Layer,
    string Description,
    bool RequiresInterface = false,
    string[] AlternativeNames = null!)
{
    public string[] AlternativeNames { get; init; } = AlternativeNames ?? [];
}