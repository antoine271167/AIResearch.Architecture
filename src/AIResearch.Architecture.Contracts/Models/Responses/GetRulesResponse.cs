using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AIResearch.Architecture.Contracts.Models.Responses;

public sealed record GetRulesResponse
{
    [JsonPropertyName("rules")]
    [Description(
        "The complete catalogue of architecture rules enforced by the system. Each entry provides a unique machine-readable rule identifier (e.g. ARCH-001, CODE-002) and a human-readable description of what the rule enforces. The AI should consult this list to understand all active constraints before generating or validating components.")]
    public required IReadOnlyCollection<ArchitectureRuleDto> Rules { get; init; }
}