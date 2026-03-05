using AIResearch.Architecture.Application.Services.Interfaces;
using AIResearch.Architecture.Contracts.Mediator;

namespace AIResearch.Architecture.Application.Queries.ValidateArchitectureQuery;

internal sealed class ValidateArchitectureQueryHandler(
    ICodeAnalyzerService codeAnalyzerService,
    IDependencyValidationService dependencyValidationService)
    : IRequestHandler<ValidateArchitectureQuery, ValidateArchitectureQueryResult>
{
    public Task<ValidateArchitectureQueryResult> HandleAsync(ValidateArchitectureQuery query,
        CancellationToken cancellationToken)
    {
        // Validate layer dependencies
        var dependencies = codeAnalyzerService.AnalyzeDependencies(query.Files);
        var dependencyViolations = dependencyValidationService.Validate(dependencies);

        // Combine all violations
        var allViolations = dependencyViolations.Distinct();

        return Task.FromResult(new ValidateArchitectureQueryResult(allViolations));
    }
}