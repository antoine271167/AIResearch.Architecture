using AIResearch.Architecture.Domain.Models.Layers;
using AIResearch.Architecture.Domain.Models.Structure;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Domain.Services;

internal class ArchitectureLayersProvider : IArchitectureLayersProvider
{
    public IReadOnlyCollection<ArchitectureLayer> GetLayers() =>
    [
        new(
            nameof(LayerType.Domain),
            "Contains domain entities, value objects, domain events, domain services, aggregates, and business rules. No dependencies on other layers. Pure business logic only.",
            []
        ),
        new(
            nameof(LayerType.Application),
            "Contains use cases, commands, queries, DTOs, application services, and orchestration logic. Defines interfaces for infrastructure. Depends only on Domain layer.",
            [nameof(LayerType.Domain)]
        ),
        new(
            nameof(LayerType.Infrastructure),
            "Contains implementations of persistence (repositories, DbContext), external services, messaging, file I/O, and third-party integrations. Implements interfaces defined in Application layer.",
            [nameof(LayerType.Application), nameof(LayerType.Domain)]
        ),
        new(
            nameof(LayerType.WebApi),
            "Contains Program.cs, Startup.cs, dependency injection container configuration, middleware pipeline, API controllers/endpoints, and application entry point. Top-level composition root.",
            [nameof(LayerType.Infrastructure), nameof(LayerType.Application), nameof(LayerType.Domain)]
        )
    ];
}