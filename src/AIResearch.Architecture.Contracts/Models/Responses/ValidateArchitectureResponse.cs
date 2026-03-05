using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AIResearch.Architecture.Contracts.Models.Responses;

public sealed record ValidateArchitectureResponse
{
    [JsonPropertyName("violations")]
    [Description(
        "The human-readable descriptions of every distinct architecture rule violated by the submitted source files (e.g. 'Domain layer cannot depend on Infrastructure layer'). Populated by analysing inter-layer dependencies across all files in the request. An empty collection means the submitted code is fully compliant; a non-empty collection lists each breached rule that must be resolved.")]
    public required IReadOnlyCollection<string> Violations { get; init; }
}
