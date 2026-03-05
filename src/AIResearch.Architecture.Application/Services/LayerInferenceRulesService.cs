using AIResearch.Architecture.Application.Services.Interfaces;
using AIResearch.Architecture.Contracts.Models.Responses;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Application.Services;

internal class LayerInferenceRulesService(IEnumerable<ILayerInferenceRule> rules) : ILayerInferenceRulesService
{
    private readonly IEnumerable<ILayerInferenceRule> _rules = rules.ToArray();

    public string? GetDeterministicLayer(string componentRole) =>
        _rules
            .FirstOrDefault(rule => rule.AppliesToComponentRole(componentRole))
            ?.LayerName;

    public IEnumerable<LayerInferenceOptionDto> GetAmbiguousCandidates(string componentRole) =>
        _rules
            .Where(rule => rule.CouldApplyToComponentRole(componentRole))
            .Select(rule => new LayerInferenceOptionDto
            {
                Layer = rule.LayerName,
                Meaning = rule.LayerDescription,
                SuggestedTypes = rule.GetSuggestedComponentRoles(componentRole)
            });
}
