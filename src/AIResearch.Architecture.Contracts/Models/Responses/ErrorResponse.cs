using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AIResearch.Architecture.Contracts.Models.Responses;

public sealed record ErrorResponse
{
    [JsonPropertyName("error")]
    [Description(
        "A human-readable message describing the error that occurred. Always present in the response and intended to be surfaced directly to the caller for diagnostics or display.")]
    public required string Error { get; init; }

    [JsonPropertyName("stackTrace")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description(
        "The full stack trace captured at the point of failure. Omitted from the response when null. Included only in non-production or debug environments to aid diagnostics without leaking internals to end users.")]
    public string? StackTrace { get; init; }
}