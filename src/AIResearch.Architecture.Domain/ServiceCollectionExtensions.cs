using AIResearch.Architecture.Domain.Services;
using AIResearch.Architecture.Domain.Services.Interfaces;
using AIResearch.Architecture.Domain.Services.Rules.CodeFeatures;
using AIResearch.Architecture.Domain.Services.Rules.Dependency;
using AIResearch.Architecture.Domain.Services.Rules.Inference;
using AIResearch.Architecture.Domain.Services.Rules.TypeKind;
using Microsoft.Extensions.DependencyInjection;

namespace AIResearch.Architecture.Domain;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddSingleton<IArchitectureLayersProvider, ArchitectureLayersProvider>();
        services.AddSingleton<IComponentRoleProvider, ComponentRoleProvider>();

        // Register layer dependency rules
        services.AddSingleton<IArchitectureRule, DomainInfrastructureDependencyRule>();
        services.AddSingleton<IArchitectureRule, ApplicationDomainDependencyRule>();
        services.AddSingleton<IArchitectureRule, InfrastructureDependencyRule>();

        // Register layer inference rules (shared instance for both interfaces)
        services.AddSingleton<InferenceApplicationRule>();
        services.AddSingleton<ILayerInferenceRule>(sp => sp.GetRequiredService<InferenceApplicationRule>());
        services.AddSingleton<IArchitectureRule>(sp => sp.GetRequiredService<InferenceApplicationRule>());

        services.AddSingleton<InferenceDomainRule>();
        services.AddSingleton<ILayerInferenceRule>(sp => sp.GetRequiredService<InferenceDomainRule>());
        services.AddSingleton<IArchitectureRule>(sp => sp.GetRequiredService<InferenceDomainRule>());

        services.AddSingleton<InferenceInfrastructureRule>();
        services.AddSingleton<ILayerInferenceRule>(sp => sp.GetRequiredService<InferenceInfrastructureRule>());
        services.AddSingleton<IArchitectureRule>(sp => sp.GetRequiredService<InferenceInfrastructureRule>());

        // Register type kind rules (shared instance for both interfaces, order matters - more specific rules first)
        services.AddSingleton<InterfaceTypeKindRule>();
        services.AddSingleton<ITypeKindRule>(sp => sp.GetRequiredService<InterfaceTypeKindRule>());
        services.AddSingleton<IArchitectureRule>(sp => sp.GetRequiredService<InterfaceTypeKindRule>());

        services.AddSingleton<RecordTypeKindRule>();
        services.AddSingleton<ITypeKindRule>(sp => sp.GetRequiredService<RecordTypeKindRule>());
        services.AddSingleton<IArchitectureRule>(sp => sp.GetRequiredService<RecordTypeKindRule>());

        services.AddSingleton<ClassTypeKindRule>();
        services.AddSingleton<ITypeKindRule>(sp => sp.GetRequiredService<ClassTypeKindRule>());
        services.AddSingleton<IArchitectureRule>(sp => sp.GetRequiredService<ClassTypeKindRule>());

        // Register code feature rules
        services.AddSingleton<InterfaceRequiredRule>();
        services.AddSingleton<ICodeFeatureRule>(sp => sp.GetRequiredService<InterfaceRequiredRule>());
        services.AddSingleton<IArchitectureRule>(sp => sp.GetRequiredService<InterfaceRequiredRule>());

        services.AddSingleton<InternalRequiredRule>();
        services.AddSingleton<IArchitectureRule>(sp => sp.GetRequiredService<InternalRequiredRule>());

        return services;
    }
}