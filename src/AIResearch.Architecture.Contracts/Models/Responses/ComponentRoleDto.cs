using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AIResearch.Architecture.Contracts.Models.Responses;

public sealed record ComponentRoleDto
{
    [JsonPropertyName("name")]
    [Description("The unique name of the component role (e.g. Entity, Command, QueryHandler).")]
    public required string Name { get; init; }

    [JsonPropertyName("layer")]
    [Description("The architecture layer this component role belongs to (e.g. Domain, Application, Infrastructure).")]
    public required string Layer { get; init; }

    [JsonPropertyName("typeKind")]
    [Description("The C# type kind used for this component role (e.g. Class, Record, Interface).")]
    public required string TypeKind { get; init; }

    [JsonPropertyName("description")]
    [Description("A human-readable explanation of the component role's purpose and responsibilities.")]
    public required string Description { get; init; }

    [JsonPropertyName("requiresInterface")]
    [Description("Indicates whether this component role requires a corresponding interface to be generated.")]
    public required bool RequiresInterface { get; init; }

    [JsonPropertyName("alternativeNames")]
    [Description("Alternative names that can be used to refer to this component role.")]
    public required IReadOnlyCollection<string> AlternativeNames { get; init; }
}
