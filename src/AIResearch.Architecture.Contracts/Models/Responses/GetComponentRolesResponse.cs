using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AIResearch.Architecture.Contracts.Models.Responses;

public sealed record GetComponentRolesResponse
{
    [JsonPropertyName("componentRoles")]
    [Description(
        "The complete catalogue of component roles available in the architecture. Each entry describes a named role, the layer it belongs to, the expected C# type kind, and whether a corresponding interface is required.")]
    public required IReadOnlyCollection<ComponentRoleDto> ComponentRoles { get; init; }
}
