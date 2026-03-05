using System.ComponentModel;

namespace AIResearch.Architecture.Contracts.Models.Requests;

public sealed record GenerateWorkspaceRequest
{
    [Description(
        "The name of the .NET solution to scaffold (without the .sln extension, e.g. MyCompany.OrderManagement). Used as the root namespace prefix and the solution file name for all generated projects.")]
    public required string SolutionName { get; init; }

    [Description(
        "The architectural style to apply to the generated workspace (Layered, CleanArchitecture, Hexagonal, EventDriven, ModularMonolith). Controls the folder structure, project layout, and layer conventions. Defaults to CleanArchitecture when omitted.")]
    public ArchitectureStyle ArchitectureStyle { get; init; } = ArchitectureStyle.CleanArchitecture;

    [Description(
        "The logical features or bounded contexts to scaffold within the solution (e.g. Order, Payment). Each entry controls namespace generation and determines which layer projects are created. If omitted, a single default context is assumed.")]
    public required IReadOnlyCollection<FeatureDefinitionDto> Features { get; init; }

    [Description(
        "The .NET target framework moniker applied to all generated projects (e.g. net10.0). Controls the <TargetFramework> element in every scaffolded .csproj file. Defaults to net8.0 when omitted.")]
    public string TargetFramework { get; init; } = "net8.0";
}