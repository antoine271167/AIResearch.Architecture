using AIResearch.Architecture.Application.Services.Interfaces;
using AIResearch.Architecture.Domain.Models.Types;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Application.Services;

internal sealed class TypeMetadataResolverService(
    IEnumerable<ITypeKindRule> typeKindRules,
    IArchitectureRulesProvider architectureRulesProvider) : ITypeMetadataResolverService
{
    private readonly ICodeFeatureRule? _interfaceRequiredRule = architectureRulesProvider
        .GetRules()
        .OfType<ICodeFeatureRule>()
        .FirstOrDefault(r => r.Id == "CODE-001");

    private readonly ICodeFeatureRule? _internalRequiredRule = architectureRulesProvider
        .GetRules()
        .OfType<ICodeFeatureRule>()
        .FirstOrDefault(r => r.Id == "CODE-002");

    public CSharpTypeKind ResolveTypeKind(string componentRole)
    {
        var matchingRule = typeKindRules.FirstOrDefault(rule => rule.AppliesToComponentRole(componentRole));

        return matchingRule?.GetTypeKind(componentRole) ?? CSharpTypeKind.Class;
    }

    public string ResolveAccessibility(string componentRole)
    {
        if (_internalRequiredRule == null)
        {
            return "public"; // Default to public if rule not found
        }

        var shouldBeInternal = _internalRequiredRule.IsRequired(componentRole);
        return shouldBeInternal ? "internal" : "public";
    }

    public bool RequiresInterface(string componentRole) =>
        _interfaceRequiredRule != null &&
        // Default to false if rule not found
        _interfaceRequiredRule.IsRequired(componentRole);
}