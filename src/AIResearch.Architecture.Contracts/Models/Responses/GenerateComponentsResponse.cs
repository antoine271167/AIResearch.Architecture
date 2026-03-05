using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AIResearch.Architecture.Contracts.Models.Responses;

public sealed record GenerateComponentsResponse
{
    [JsonPropertyName("status")]
    [Description(
        "The outcome of the code generation operation. One of: 'ok' (all components generated successfully), 'invalid' (validation or architecture rule violation), or 'ambiguous' (the target layer could not be unambiguously inferred). Inspect this field first to determine which other fields are populated.")]
    public required string Status { get; init; }

    [JsonPropertyName("files")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description(
        "A dictionary of generated files keyed by their relative file path, with the generated C# source code as the value. Populated only when Status is 'ok'. Each entry represents one scaffolded component file ready to be written to disk.")]
    public Dictionary<string, string>? Files { get; init; }

    [JsonPropertyName("component")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description(
        "The Name of the specific component (from CodeGenerationRequestDto) that triggered the failure. Populated when Status is 'invalid' or 'ambiguous' to identify which component in the request caused the problem.")]
    public string? Component { get; init; }

    [JsonPropertyName("reason")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description(
        "A human-readable explanation of why the operation failed or could not be completed. Populated when Status is 'invalid' or 'ambiguous'. For 'ambiguous', explains why the layer could not be inferred and should be read alongside Options.")]
    public string? Reason { get; init; }

    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description(
        "Supplementary context or instruction accompanying an architecture rule violation. Populated alongside Violations when Status is 'invalid' due to a dependency rule breach, providing a human-readable summary of the violation.")]
    public string? Message { get; init; }

    [JsonPropertyName("options")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description(
        "Candidate layer inference options returned when Status is 'ambiguous'. Each entry describes a possible target layer, its responsibilities, and suggested component roles. The AI should use this information to pick an explicit layer and retry the request with the Layer field set.")]
    public IReadOnlyCollection<LayerInferenceOptionDto>? Options { get; init; }

    [JsonPropertyName("violations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description(
        "The architecture rules violated by the requested component placement. Populated when Status is 'invalid' due to a dependency rule breach. Each entry represents a single violated rule that must be resolved before the component can be generated.")]
    public IEnumerable<object>? Violations { get; init; }
}