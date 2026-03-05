using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AIResearch.Architecture.Contracts.Models.Responses;

public sealed record ValidationErrorResponse
{
    [JsonPropertyName("status")]
    [Description(
        "A fixed discriminator value that identifies this response as a structural request error. Always set to 'validation_error', indicating the request was rejected before any business logic was executed due to a malformed or missing request body.")]
    public required string Status { get; init; }

    [JsonPropertyName("message")]
    [Description(
        "A human-readable explanation of why the request body was rejected, including a description of the expected format and an example of a valid payload. Intended to give the caller enough information to correct the request and retry.")]
    public required string Message { get; init; }
}
