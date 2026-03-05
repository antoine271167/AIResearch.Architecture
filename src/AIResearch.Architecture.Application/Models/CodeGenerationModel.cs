namespace AIResearch.Architecture.Application.Models;

/// <summary>
/// Internal model for code generation, mapped from the external contract.
/// </summary>
internal sealed record CodeGenerationModel(
    string ComponentRole,
    string Name,
    string SolutionName,
    string Layer,
    string Feature,
    IReadOnlyCollection<string> Commands,
    string Comments,
    IReadOnlyCollection<string> Dependencies,
    IReadOnlyCollection<string> ImplementsInterfaces
);
