using AIResearch.Architecture.Contracts.Mediator;
using AIResearch.Architecture.Contracts.Models.Requests;

namespace AIResearch.Architecture.Application.Commands.GenerateComponents;

public sealed record GenerateComponentsCommand(
    GenerateComponentsRequest Request
) : IRequest<GenerateComponentsResult>;