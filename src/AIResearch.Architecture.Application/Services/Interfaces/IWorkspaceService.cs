using AIResearch.Architecture.Application.Commands.GenerateWorkspace;
using AIResearch.Architecture.Contracts.Models.Requests;

namespace AIResearch.Architecture.Application.Services.Interfaces;

internal interface IWorkspaceService
{
    GenerateWorkspaceResult Generate(GenerateWorkspaceRequest request);
}