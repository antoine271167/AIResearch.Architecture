namespace AIResearch.Architecture.Domain.Models.Interfaces;

/// <summary>
///     Represents an interface requirement analysis result for a component.
/// </summary>
public sealed record InterfaceRequirement(
    string ComponentName,
    string ComponentRole,
    string FilePath,
    bool HasInterface,
    IReadOnlyCollection<string> ImplementedInterfaces);