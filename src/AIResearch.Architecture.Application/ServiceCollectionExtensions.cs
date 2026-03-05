using AIResearch.Architecture.Application.Commands.GenerateComponents;
using AIResearch.Architecture.Application.Commands.GenerateWorkspace;
using AIResearch.Architecture.Application.Queries.GetArchitecture;
using AIResearch.Architecture.Application.Queries.GetComponentRoles;
using AIResearch.Architecture.Application.Queries.GetRulesQuery;
using AIResearch.Architecture.Application.Queries.ValidateArchitectureQuery;
using AIResearch.Architecture.Application.Services;
using AIResearch.Architecture.Application.Services.Interfaces;
using AIResearch.Architecture.Contracts.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace AIResearch.Architecture.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<INamespaceService, NamespaceService>();
        services.AddScoped<IFilePathService, FilePathService>();
        services.AddScoped<ICodeGeneratorService, CSharpCodeGeneratorService>();
        services.AddScoped<ICodeAnalyzerService, CSharpCodeAnalyzerService>();
        services.AddScoped<IWorkspaceService, WorkspaceService>();
        services.AddSingleton<IArchitectureRulesProvider, ArchitectureRulesProvider>();
        services.AddScoped<ILayerInferenceService, LayerInferenceService>();
        services.AddScoped<ILayerInferenceRulesService, LayerInferenceRulesService>();
        services.AddScoped<IDependencyAnalysisService, DependencyAnalysisService>();
        services.AddScoped<IDependencyValidationService, DependencyValidationService>();
        services.AddScoped<ILayerDependencyService, LayerDependencyService>();
        services.AddScoped<ITypeMetadataResolverService, TypeMetadataResolverService>();

        // Register Mediator
        services.AddScoped<IMediator, Mediator.Mediator>();

        // Register Handlers
        services
            .AddScoped<IRequestHandler<GetArchitectureQuery, GetArchitectureQueryResult>,
                GetArchitectureQueryHandler>();
        services.AddScoped<IRequestHandler<GetRulesQuery, GetRulesQueryResult>, GetRulesQueryHandler>();
        services
            .AddScoped<IRequestHandler<GetComponentRolesQuery, GetComponentRolesQueryResult>,
                GetComponentRolesQueryHandler>();
        services
            .AddScoped<IRequestHandler<ValidateArchitectureQuery, ValidateArchitectureQueryResult>,
                ValidateArchitectureQueryHandler>();
        services
            .AddScoped<IRequestHandler<GenerateComponentsCommand, GenerateComponentsResult>,
                GenerateComponentsCommandHandler>();
        services
            .AddScoped<IRequestHandler<GenerateWorkspaceCommand, GenerateWorkspaceResult>,
                GenerateWorkspaceCommandHandler>();

        return services;
    }
}