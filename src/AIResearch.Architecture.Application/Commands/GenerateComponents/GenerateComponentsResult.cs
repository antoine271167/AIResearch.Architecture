using AIResearch.Architecture.Contracts.Models;
using AIResearch.Architecture.Contracts.Models.Responses;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Application.Commands.GenerateComponents;

public sealed record GenerateComponentsResult(
    string Status,
    string? Component = null,
    string? Reason = null,
    string? Message = null,
    List<FileContent>? Files = null,
    IReadOnlyCollection<LayerInferenceOptionDto>? Options = null,
    IEnumerable<IArchitectureRule>? Violations = null);