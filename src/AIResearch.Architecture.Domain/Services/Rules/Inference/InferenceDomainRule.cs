using AIResearch.Architecture.Domain.Models.Layers;
using AIResearch.Architecture.Domain.Services.Interfaces;
using AIResearch.Architecture.Domain.Services.Rules.Base;

namespace AIResearch.Architecture.Domain.Services.Rules.Inference;

internal class InferenceDomainRule(
    IArchitectureLayersProvider layersProvider,
    IComponentRoleProvider roleProvider)
    : LayerInferenceRuleBase(layersProvider, roleProvider, LayerType.Domain)
{
    public override string Id => "INFE-002";
}