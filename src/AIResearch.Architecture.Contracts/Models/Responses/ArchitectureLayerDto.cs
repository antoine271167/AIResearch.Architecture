using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AIResearch.Architecture.Contracts.Models.Responses;

public sealed record ArchitectureLayerDto
{
    [JsonPropertyName("name")]
    [Description("The unique identifier of the architecture layer (e.g. Domain, Application, Infrastructure).")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    [Description("A human-readable explanation of the layer's responsibilities and purpose.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }

    [JsonPropertyName("allowedDependencies")]
    [Description(
        "The names of other layers this layer is permitted to depend on, enforcing strict architectural boundaries.")]
    public required IReadOnlyCollection<string> AllowedDependencies { get; init; }
}