using AIResearch.Architecture.Contracts.Mediator;

namespace AIResearch.Architecture.Application.Queries.GetArchitecture;

public sealed record GetArchitectureQuery : IRequest<GetArchitectureQueryResult>;