using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AIResearch.Architecture.Contracts.Models.Requests;

public sealed record FeatureDefinitionDto
{
    [JsonPropertyName("name")]
    [Description(
        "The logical name of the feature or bounded context (e.g. Order, Payment). Used as the organizing unit for namespace and project path generation for all components within this feature.")]
    public required string Name { get; init; }

    [JsonPropertyName("applicationKind")]
    [Description(
        "The application type of this feature (WebApi, Worker, Console, Library). Controls which layer projects are scaffolded — the Host project is only generated for WebApi. Defaults to WebApi when omitted.")]
    public ApplicationKind ApplicationKind { get; init; }
}