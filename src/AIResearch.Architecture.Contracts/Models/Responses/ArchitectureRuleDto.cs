using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AIResearch.Architecture.Contracts.Models.Responses;

public sealed record ArchitectureRuleDto
{
    [JsonPropertyName("id")]
    [Description("A unique machine-readable identifier for the architecture rule (e.g. ARCH-001, CODE-002).")]
    public required string Id { get; init; }

    [JsonPropertyName("description")]
    [Description("A human-readable explanation of what the rule enforces and when it applies.")]
    public required string Description { get; init; }
}