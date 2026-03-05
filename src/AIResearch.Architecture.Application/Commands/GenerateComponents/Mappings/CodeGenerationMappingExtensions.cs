using AIResearch.Architecture.Application.Models;
using AIResearch.Architecture.Contracts.Models.Requests;
using AIResearch.Architecture.Domain.Models.Layers;

namespace AIResearch.Architecture.Application.Commands.GenerateComponents.Mappings;

internal static class CodeGenerationMappingExtensions
{
    extension(CodeGenerationRequestDto component)
    {
        public LayerInferenceRequest ToLayerInferenceRequest(GenerateComponentsRequest request,
            FeatureDefinitionDto feature) =>
            new(
                component.ComponentRole,
                component.Name,
                request.SolutionName,
                feature.Name,
                component.Commands ?? [],
                component.Comments ?? "",
                component.ImplementsInterfaces ?? []);

        public CodeGenerationModel ToCodeGenerationModel(GenerateComponentsRequest request,
            FeatureDefinitionDto feature,
            string layerName) =>
            new(
                component.ComponentRole,
                component.Name,
                request.SolutionName,
                layerName,
                feature.Name,
                component.Commands ?? [],
                component.Comments ?? "",
                component.Dependencies ?? [],
                component.ImplementsInterfaces ?? []);

        public LayerDependency ToLayerDependency(string layerName) =>
            new(layerName, (component.Dependencies ?? []).ToArray());
    }
}