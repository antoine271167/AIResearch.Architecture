using System.ComponentModel;

namespace AIResearch.Architecture.Contracts.Models.Requests;

public sealed record GenerateComponentsRequest
{
    [Description(
        "The name of the .NET solution being generated (e.g. MyCompany.OrderManagement). Used as the root namespace prefix and the solution file name for all scaffolded projects and components.")]
    public required string SolutionName { get; init; }

    [Description(
        "The set of features or bounded contexts to scaffold (e.g. Order, Payment). Each entry controls namespace generation and determines which layer projects are created for the components that belong to it.")]
    public required IReadOnlyCollection<FeatureDefinitionDto> Features { get; init; }

    [Description(
        "The individual component generation requests to process (e.g. CommandHandler, Repository, Entity). Each entry describes a single C# type to generate, including its role, name, layer placement, dependencies, and interfaces.")]
    public required IReadOnlyCollection<CodeGenerationRequestDto> Components { get; init; }
}