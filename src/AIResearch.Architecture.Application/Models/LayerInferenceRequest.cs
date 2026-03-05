namespace AIResearch.Architecture.Application.Models;

internal sealed record LayerInferenceRequest(
    string ComponentRole,
    string Name,
    string SolutionName,
    string Feature,
    IReadOnlyCollection<string> Commands,
    string Comments = "",
    IReadOnlyCollection<string>? ImplementsInterfaces = null
);
