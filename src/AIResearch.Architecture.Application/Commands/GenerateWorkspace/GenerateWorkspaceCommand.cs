using AIResearch.Architecture.Contracts.Mediator;
using AIResearch.Architecture.Contracts.Models.Requests;

namespace AIResearch.Architecture.Application.Commands.GenerateWorkspace;

public sealed record GenerateWorkspaceCommand(
    GenerateWorkspaceRequest Request
) : IRequest<GenerateWorkspaceResult>;