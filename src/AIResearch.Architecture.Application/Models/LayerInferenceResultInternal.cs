using AIResearch.Architecture.Contracts.Models.Responses;

namespace AIResearch.Architecture.Application.Models;

internal sealed record LayerInferenceResultInternal(
    LayerInferenceStatusInternal Status,
    CodeGenerationModel? Model,
    AmbiguityDetailsInternal? Ambiguity,
    string OriginalName
);

internal enum LayerInferenceStatusInternal
{
    Ok,
    Ambiguous,
    Invalid
}

internal sealed record AmbiguityDetailsInternal(
    string Reason,
    IReadOnlyCollection<LayerInferenceOptionDto> Options
);
