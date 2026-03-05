using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AIResearch.Architecture.Contracts.Models.Responses;

public sealed record GetArchitectureResponse
{
    [JsonPropertyName("name")]
    [Description(
        "The human-readable name of the architecture descriptor (e.g. Clean Architecture). Identifies the architectural approach being described and is intended to be displayed to the user or logged for traceability.")]
    public required string Name { get; init; }

    [JsonPropertyName("version")]
    [Description(
        "The version of the architecture descriptor (e.g. 1.0). Allows consumers and tooling to detect changes to the layer definitions or dependency rules between releases.")]
    public required string Version { get; init; }

    [JsonPropertyName("description")]
    [Description(
        "A human-readable summary of the architecture's goals, principles, and constraints. Provides the AI with the conceptual context needed to understand why the layer structure and dependency rules are defined as they are.")]
    public required string Description { get; init; }

    [JsonPropertyName("style")]
    [Description(
        "The architectural style of this descriptor serialised as a string (e.g. CleanArchitecture, Layered, Hexagonal, EventDriven, ModularMonolith). Decouples the consumer from the ArchitectureStyle enum while still conveying the applied pattern.")]
    public required string Style { get; init; }

    [JsonPropertyName("layers")]
    [Description(
        "The complete ordered catalogue of architecture layers defined by this descriptor. Each entry contains the layer name, its responsibilities, and the layers it is permitted to depend on. This is the primary payload the AI consults before generating or validating components.")]
    public required IReadOnlyCollection<ArchitectureLayerDto> Layers { get; init; }
}