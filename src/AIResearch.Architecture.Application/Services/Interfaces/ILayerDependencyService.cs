namespace AIResearch.Architecture.Application.Services.Interfaces;

internal interface ILayerDependencyService
{
    string[] GetAllowedDependencies(string layerName);
    bool IsValidDependency(string sourceLayer, string targetLayer);
}
