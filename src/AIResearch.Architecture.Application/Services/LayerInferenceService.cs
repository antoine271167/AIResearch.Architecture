using AIResearch.Architecture.Application.Models;
using AIResearch.Architecture.Application.Services.Interfaces;
using AIResearch.Architecture.Contracts.Models.Responses;

namespace AIResearch.Architecture.Application.Services;

internal sealed class LayerInferenceService(
    ILayerInferenceRulesService layerInferenceRulesService,
    ILayerDependencyService layerDependencyService) : ILayerInferenceService
{
    public LayerInferenceResultInternal InferRequest(LayerInferenceRequest request)
    {
        var deterministicLayer = layerInferenceRulesService.GetDeterministicLayer(request.ComponentRole);
        if (deterministicLayer is not null)
        {
            return CreateSuccessResult(request, deterministicLayer);
        }

        var candidates = layerInferenceRulesService
            .GetAmbiguousCandidates(request.ComponentRole)
            .ToList();

        return candidates.Count > 0
            ? CreateAmbiguousResult(request, candidates)
            : CreateInvalidResult(request);
    }

    private LayerInferenceResultInternal CreateSuccessResult(LayerInferenceRequest request, string deterministicLayer)
    {
        var allowedDependencies = layerDependencyService.GetAllowedDependencies(deterministicLayer);
        var codeGenerationModel = CreateCodeGenerationModel(request, deterministicLayer, allowedDependencies);

        return new LayerInferenceResultInternal(
            LayerInferenceStatusInternal.Ok,
            codeGenerationModel,
            null,
            request.Name
        );
    }

    private static CodeGenerationModel CreateCodeGenerationModel(
        LayerInferenceRequest request,
        string layer,
        IEnumerable<string> allowedDependencies) =>
        new(
            request.ComponentRole,
            request.Name,
            request.SolutionName,
            layer,
            request.Feature,
            request.Commands,
            request.Comments,
            allowedDependencies.ToList(),
            request.ImplementsInterfaces ?? []);

    private static LayerInferenceResultInternal CreateAmbiguousResult(
        LayerInferenceRequest request,
        List<LayerInferenceOptionDto> candidates)
    {
        var ambiguityDetails = new AmbiguityDetailsInternal(
            $"The `component role '{request.ComponentRole}' is architecturally ambiguous. " +
            $"Select one of the provided options.",
            candidates
        );

        return new LayerInferenceResultInternal(
            LayerInferenceStatusInternal.Ambiguous,
            null,
            ambiguityDetails,
            request.Name
        );
    }

    private static LayerInferenceResultInternal CreateInvalidResult(LayerInferenceRequest request)
    {
        var ambiguityDetails = new AmbiguityDetailsInternal(
            $"The layer could not be inferred from the component role '{request.ComponentRole}'. Provide a correct role or provide the layer.",
            []
        );

        return new LayerInferenceResultInternal(
            LayerInferenceStatusInternal.Invalid,
            null,
            ambiguityDetails,
            request.Name
        );
    }
}