using AIResearch.Architecture.Application.Services.Interfaces;
using AIResearch.Architecture.Contracts.Mediator;
using AIResearch.Architecture.Contracts.Models.Requests;

namespace AIResearch.Architecture.Application.Commands.GenerateWorkspace;

internal sealed class GenerateWorkspaceCommandHandler(
    IWorkspaceService workspaceService)
    : IRequestHandler<GenerateWorkspaceCommand, GenerateWorkspaceResult>
{
    public Task<GenerateWorkspaceResult> HandleAsync(GenerateWorkspaceCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = ValidateRequest(request.Request);
        if (validationResult is not null)
        {
            return Task.FromResult(validationResult);
        }

        var workspace = workspaceService.Generate(request.Request);
        return Task.FromResult(workspace);
    }

    private static GenerateWorkspaceResult? ValidateRequest(GenerateWorkspaceRequest request)
    {
        if (request.Features.Count == 0)
        {
            return new GenerateWorkspaceResult(
                "invalid",
                Reason: "Features must contain at least one item. Please specify at least one feature definition."
            );
        }

        return null;
    }
}