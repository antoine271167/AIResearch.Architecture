using AIResearch.Architecture.Application.Services.Interfaces;
using AIResearch.Architecture.Domain.Models.Layers;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Application.Services;

internal class DependencyValidationService(IArchitectureRulesProvider architectureRulesProvider)
    : IDependencyValidationService
{
    private readonly IEnumerable<ILayerDependencyRule>
        _rules = architectureRulesProvider.GetRules().OfType<ILayerDependencyRule>();

    public IEnumerable<IArchitectureRule> Validate(IEnumerable<LayerDependency> dependencies) =>
        dependencies.SelectMany(layer => layer.DependsOn, (layer, dep) => new { layer, dep })
            .SelectMany(_ => _rules, (t, rule) => new { t, rule })
            .Where(t => t.rule.IsViolated(t.t.layer.LayerName, t.t.dep))
            .Select(IArchitectureRule (t) => t.rule).Distinct();
}
