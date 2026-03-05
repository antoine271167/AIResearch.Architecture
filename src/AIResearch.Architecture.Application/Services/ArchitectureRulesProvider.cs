using AIResearch.Architecture.Application.Services.Interfaces;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Application.Services;

internal class ArchitectureRulesProvider(IEnumerable<IArchitectureRule> rules) : IArchitectureRulesProvider
{
    public IEnumerable<IArchitectureRule> GetRules() => rules;
}
