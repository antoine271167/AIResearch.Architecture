using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Application.Services.Interfaces;

public interface IArchitectureRulesProvider
{
    IEnumerable<IArchitectureRule> GetRules();
}
