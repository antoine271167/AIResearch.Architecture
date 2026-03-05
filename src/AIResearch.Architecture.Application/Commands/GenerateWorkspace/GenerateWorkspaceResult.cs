using AIResearch.Architecture.Contracts.Models;

namespace AIResearch.Architecture.Application.Commands.GenerateWorkspace;

public sealed record GenerateWorkspaceResult(
    string Status,
    IReadOnlyCollection<FileContent>? Files = null,
    string? Reason = null
);