using AIResearch.Architecture.Domain.Models.Layers;
using AIResearch.Architecture.Domain.Models.Roles;
using AIResearch.Architecture.Domain.Models.Types;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Domain.Services;

/// <summary>
///     Centralized provider for component role definitions and their metadata.
///     This is the single source of truth for all component role configurations.
/// </summary>
internal sealed class ComponentRoleProvider : IComponentRoleProvider
{
    public ComponentRoleProvider()
    {
        _roleCache = _allRoles
            .ToDictionary(role => role.Name, role => role, StringComparer.OrdinalIgnoreCase);
    }

    private static readonly ComponentRoleMetadata[] _allRoles =
    [
        // Application Layer - Services
        new("ApplicationService", CSharpTypeKind.Class, LayerType.Application,
            "Application service that orchestrates use cases and business workflows",
            RequiresInterface: true),

        // Application Layer - CQRS Command
        new("Command", CSharpTypeKind.Record, LayerType.Application,
            "Command object representing an intent to change state"),
        new("CommandHandler", CSharpTypeKind.Class, LayerType.Application,
            "Handler that processes commands and executes business logic",
            RequiresInterface: true),

        // Application Layer - CQRS Query
        new("Query", CSharpTypeKind.Record, LayerType.Application,
            "Query object representing a request for data"),
        new("QueryHandler", CSharpTypeKind.Class, LayerType.Application,
            "Handler that processes queries and returns data",
            RequiresInterface: true),

        // Application Layer - Events
        new("ApplicationEvent", CSharpTypeKind.Record, LayerType.Application,
            "Application-level event representing something that happened"),
        new("ApplicationEventHandler", CSharpTypeKind.Class, LayerType.Application,
            "Handler that processes application events",
            RequiresInterface : true),

        // Application Layer - Interfaces
        new("RepositoryInterface", CSharpTypeKind.Interface, LayerType.Application,
            "Interface defining repository contracts for data access"),
        new("GatewayInterface", CSharpTypeKind.Interface, LayerType.Application,
            "Interface defining gateway contracts for external service communication"),
        new("EventBusInterface", CSharpTypeKind.Interface, LayerType.Application,
            "Interface defining event bus contracts for message publishing"),
        new("ApplicationServiceInterface", CSharpTypeKind.Interface, LayerType.Application,
            "Interface defining application service contracts"),
        new("CommandHandlerInterface", CSharpTypeKind.Interface, LayerType.Application,
            "Interface defining command handler contracts"),
        new("QueryHandlerInterface", CSharpTypeKind.Interface, LayerType.Application,
            "Interface defining query handler contracts"),
        new("ApplicationEventHandlerInterface", CSharpTypeKind.Interface, LayerType.Application,
            "Interface defining application event handler contracts"),

        // Application Layer - Other
        new("Behavior", CSharpTypeKind.Class, LayerType.Application,
            "Pipeline behavior for cross-cutting concerns",
            RequiresInterface: true),
        new("ApplicationMapper", CSharpTypeKind.Class, LayerType.Application,
            "Mapper for transforming between application layer objects"),
        new("ApplicationValidator", CSharpTypeKind.Class, LayerType.Application,
            "Validator for application layer business rules",
            RequiresInterface: true),
        new("ApplicationModel", CSharpTypeKind.Record, LayerType.Application,
            "Data model used in the application layer"),

        // Domain Layer - Core
        new("Aggregate", CSharpTypeKind.Class, LayerType.Domain,
            "Aggregate root that ensures consistency boundaries"),
        new("AggregateRoot", CSharpTypeKind.Class, LayerType.Domain,
            "Root entity of an aggregate that ensures consistency"),
        new("DomainEntity", CSharpTypeKind.Class, LayerType.Domain,
            "Entity with identity in the domain model"),
        new("ValueObject", CSharpTypeKind.Record, LayerType.Domain,
            "Immutable object defined by its attributes rather than identity"),

        // Domain Layer - Services
        new("DomainService", CSharpTypeKind.Class, LayerType.Domain,
            "Service that encapsulates domain logic not belonging to entities",
            RequiresInterface: true),
        new("DomainServiceInterface", CSharpTypeKind.Interface, LayerType.Domain,
            "Interface defining domain service contracts"),

        // Domain Layer - Events
        new("DomainEvent", CSharpTypeKind.Record, LayerType.Domain,
            "Domain event representing something significant that happened in the domain"),
        new("DomainEventHandler", CSharpTypeKind.Class, LayerType.Domain,
            "Handler that processes domain events",
            RequiresInterface: true),

        // Domain Layer - Other
        new("DomainValidator", CSharpTypeKind.Class, LayerType.Domain,
            "Validator for domain business rules",
            RequiresInterface: true),
        new("DomainMapper", CSharpTypeKind.Class, LayerType.Domain,
            "Mapper for transforming between domain objects"),
        new("DomainModel", CSharpTypeKind.Record, LayerType.Domain,
            "Data model used in the domain layer"),

        // Infrastructure Layer - Persistence
        new("Repository", CSharpTypeKind.Class, LayerType.Infrastructure,
            "Implementation of repository for data access",
            RequiresInterface: true),
        new("Entity", CSharpTypeKind.Class, LayerType.Infrastructure,
            "Database entity representing a table"),

        // Infrastructure Layer - Integration
        new("Gateway", CSharpTypeKind.Class, LayerType.Infrastructure,
            "Gateway implementation for external service communication",
            RequiresInterface: true),
        new("GatewayAdapter", CSharpTypeKind.Class, LayerType.Infrastructure,
            "Adapter for external service integration",
            RequiresInterface: true),

        // Infrastructure Layer - Services
        new("InfrastructureService", CSharpTypeKind.Class, LayerType.Infrastructure,
            "Infrastructure service for technical concerns",
            RequiresInterface: true),
        new("InfrastructureServiceInterface", CSharpTypeKind.Interface, LayerType.Infrastructure,
            "Interface defining infrastructure service contracts"),

        // Infrastructure Layer - Other
        new("InfrastructureModel", CSharpTypeKind.Record, LayerType.Infrastructure,
            "Data model used in the infrastructure layer"),

        // Web API Layer
        new("Controller", CSharpTypeKind.Class, LayerType.WebApi,
            "API controller handling HTTP requests"),

        // Generic/Ambiguous Roles
        new("Interface", CSharpTypeKind.Interface, LayerType.Application,
            "Generic interface (layer must be inferred from context)",
            AlternativeNames: ["ApplicationServiceInterface", "DomainServiceInterface", "InfrastructureServiceInterface"])
    ];

    private readonly Dictionary<string, ComponentRoleMetadata> _roleCache;

    public IReadOnlyCollection<ComponentRoleMetadata> GetAllRoles() => _allRoles;

    public ComponentRoleMetadata? GetRoleMetadata(string roleName) => _roleCache.GetValueOrDefault(roleName);

    public IReadOnlyCollection<ComponentRoleMetadata> GetRoles(Func<ComponentRoleMetadata, bool> predicate) =>
        _allRoles.Where(predicate).ToArray();
}