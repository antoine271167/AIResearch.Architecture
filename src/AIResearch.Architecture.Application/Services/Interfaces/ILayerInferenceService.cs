using AIResearch.Architecture.Application.Models;

namespace AIResearch.Architecture.Application.Services.Interfaces;

internal interface ILayerInferenceService
{
    LayerInferenceResultInternal InferRequest(LayerInferenceRequest request);
}
