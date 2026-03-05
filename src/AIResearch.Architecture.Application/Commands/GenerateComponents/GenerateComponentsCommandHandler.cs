using AIResearch.Architecture.Application.Commands.GenerateComponents.Mappings;
using AIResearch.Architecture.Application.Models;
using AIResearch.Architecture.Application.Services.Interfaces;
using AIResearch.Architecture.Contracts.Mediator;
using AIResearch.Architecture.Contracts.Models;
using AIResearch.Architecture.Contracts.Models.Requests;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Application.Commands.GenerateComponents;

internal sealed class GenerateComponentsCommandHandler(
    IFilePathService filePathService,
    ILayerInferenceService layerInferenceService,
    IDependencyValidationService dependencyValidationService,
    ICodeGeneratorService codeGeneratorService,
    IArchitectureLayersProvider architectureLayersProvider,
    ITypeMetadataResolverService typeMetadataResolver)
    : IRequestHandler<GenerateComponentsCommand, GenerateComponentsResult>
{
    public Task<GenerateComponentsResult> HandleAsync(GenerateComponentsCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = ValidateRequest(request.Request);
        if (validationResult != null)
        {
            return Task.FromResult(validationResult);
        }

        var results = new List<FileContent>();

        foreach (var feature in request.Request.Features)
        {
            foreach (var component in request.Request.Components)
            {
                var result = ProcessComponent(component, request.Request, feature, results);
                if (result != null)
                {
                    return Task.FromResult(result);
                }
            }
        }

        return Task.FromResult(new GenerateComponentsResult("ok", Files: results));
    }

    private static GenerateComponentsResult? ValidateRequest(GenerateComponentsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SolutionName))
        {
            return new GenerateComponentsResult(
                "invalid",
                Reason: "SolutionName must be provided in the request. Please specify the solution name."
            );
        }

        return null;
    }

    private GenerateComponentsResult? ProcessComponent(
        CodeGenerationRequestDto component,
        GenerateComponentsRequest request,
        FeatureDefinitionDto feature,
        List<FileContent> results)
    {
        // Validate interface requirement first
        var interfaceValidationError = ValidateInterfaceRequirement(component);
        if (interfaceValidationError != null)
        {
            return interfaceValidationError;
        }

        // Validate accessibility consistency
        var accessibilityValidationError = ValidateAccessibility(component);
        if (accessibilityValidationError != null)
        {
            return accessibilityValidationError;
        }

        var modelResult = string.IsNullOrWhiteSpace(component.Layer)
            ? InferLayerAndCreateModel(component, request, feature)
            : ValidateLayerAndCreateModel(component, request, feature);

        if (modelResult.Error != null)
        {
            return modelResult.Error;
        }

        var generatedFile = GenerateFile(modelResult.Model!, request.SolutionName);
        results.Add(generatedFile);

        return null;
    }

    private (CodeGenerationModel? Model, GenerateComponentsResult? Error) InferLayerAndCreateModel(
        CodeGenerationRequestDto component,
        GenerateComponentsRequest request,
        FeatureDefinitionDto feature)
    {
        var layerInferenceRequest = component.ToLayerInferenceRequest(request, feature);
        var inference = layerInferenceService.InferRequest(layerInferenceRequest);

        return inference.Status switch
        {
            LayerInferenceStatusInternal.Ambiguous => (null, new GenerateComponentsResult(
                "ambiguous",
                component.Name,
                inference.Ambiguity!.Reason,
                Options: inference.Ambiguity.Options)),

            LayerInferenceStatusInternal.Invalid => (null, new GenerateComponentsResult(
                "invalid",
                component.Name,
                inference.Ambiguity!.Reason)),

            LayerInferenceStatusInternal.Ok => (inference.Model!, null),

            _ => throw new InvalidOperationException("Unknown inference status")
        };
    }

    private (CodeGenerationModel? Model, GenerateComponentsResult? Error) ValidateLayerAndCreateModel(
        CodeGenerationRequestDto component,
        GenerateComponentsRequest request,
        FeatureDefinitionDto feature)
    {
        var layers = architectureLayersProvider.GetLayers();
        var layer = layers.FirstOrDefault(l =>
            l.Name.Equals(component.Layer, StringComparison.OrdinalIgnoreCase));

        if (layer == null)
        {
            return (null, new GenerateComponentsResult(
                "invalid",
                component.Name,
                $"Unknown layer: {component.Layer}. Please specify a valid layer."));
        }

        var validationError = ValidateDependencies(component, layer.Name);
        if (validationError != null)
        {
            return (null, validationError);
        }

        var model = component.ToCodeGenerationModel(request, feature, layer.Name);

        return (model, null);
    }

    private GenerateComponentsResult? ValidateDependencies(CodeGenerationRequestDto component, string layerName)
    {
        var layerDependencies = new[] { component.ToLayerDependency(layerName) };

        var violations = dependencyValidationService.Validate(layerDependencies);

        if (violations.Any())
        {
            return new GenerateComponentsResult(
                "invalid",
                Message: $"Component {component.Name} violates architecture rules.",
                Violations: violations);
        }

        return null;
    }

    private GenerateComponentsResult? ValidateInterfaceRequirement(CodeGenerationRequestDto component)
    {
        var requiresInterface = typeMetadataResolver.RequiresInterface(component.ComponentRole);

        if (!requiresInterface)
        {
            return null;
        }

        var hasInterface = component.ImplementsInterfaces is { Count: > 0 };

        if (hasInterface)
        {
            return null;
        }

        var suggestedInterfaceName = $"I{component.Name}";
        return new GenerateComponentsResult(
            "invalid",
            component.Name,
            $"Component role '{component.ComponentRole}' requires an interface implementation (CODE-001). " +
            "Please specify the interface in 'ImplementsInterfaces' property and request the creation of this interface. " +
            $"Suggested interface name: '{suggestedInterfaceName}'.");
    }

    private GenerateComponentsResult? ValidateAccessibility(CodeGenerationRequestDto component)
    {
        var requiresInterface = typeMetadataResolver.RequiresInterface(component.ComponentRole);
        var accessibility = typeMetadataResolver.ResolveAccessibility(component.ComponentRole);

        if (requiresInterface && accessibility != "internal")
        {
            return new GenerateComponentsResult(
                "invalid",
                component.Name,
                $"Component role '{component.ComponentRole}' must be internal as Components " +
                "that are not interfaces but require an interface implementation should be internal (CODE-002).");
        }

        return null;
    }

    private FileContent GenerateFile(CodeGenerationModel model, string solutionName)
    {
        var code = codeGeneratorService.GenerateCode(model);
        var filePath = filePathService.GetSourceFilePath(solutionName, model.Layer, model.Name, model.Feature);

        return new FileContent { Path = filePath, Code = code };
    }
}