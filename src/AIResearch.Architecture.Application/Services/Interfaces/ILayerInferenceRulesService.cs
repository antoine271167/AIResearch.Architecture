using AIResearch.Architecture.Contracts.Models.Responses;

namespace AIResearch.Architecture.Application.Services.Interfaces;

public interface ILayerInferenceRulesService
{
    string? GetDeterministicLayer(string componentRole);
    IEnumerable<LayerInferenceOptionDto> GetAmbiguousCandidates(string componentRole);
}
