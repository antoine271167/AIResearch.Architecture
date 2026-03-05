using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AIResearch.Architecture.Contracts.Models.Responses;

public sealed record GenerateWorkspaceResponse
{
    [JsonPropertyName("status")]
    [Description(
        "The outcome of the workspace generation operation. One of: 'ok' (the solution and all project files were generated successfully) or 'invalid' (request validation failed, e.g. the Features collection was empty). Inspect this field first to determine which other fields are populated.")]
    public required string Status { get; init; }

    [JsonPropertyName("files")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description(
        "A dictionary of generated workspace files keyed by their relative file path, with the file content as the value. Populated only when Status is 'ok'. Contains the scaffolded .sln file and one .csproj file per architecture layer per feature, ready to be written to disk.")]
    public Dictionary<string, string>? Files { get; init; }

    [JsonPropertyName("reason")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description(
        "A human-readable explanation of why the request was rejected. Populated only when Status is 'invalid'. Describes the validation rule that was violated and provides guidance on how to correct the request before retrying.")]
    public string? Reason { get; init; }
}
