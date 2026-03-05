using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AIResearch.Architecture.Contracts.Models.Responses;

public sealed record LayerInferenceOptionDto
{
    [JsonPropertyName("layer")]
    [Description("The name of the candidate architecture layer this component role could be placed in.")]
    public required string Layer { get; init; }

    [JsonPropertyName("meaning")]
    [Description(
        "A human-readable explanation of the candidate layer's responsibilities, helping the AI understand what belongs there.")]
    public required string Meaning { get; init; }

    [JsonPropertyName("suggestedTypes")]
    [Description(
        "Canonical component role names within this layer that match or are related to the ambiguous role provided by the AI.")]
    public required IReadOnlyCollection<string> SuggestedTypes { get; init; }
}