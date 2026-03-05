using AIResearch.Architecture.Domain.Models.Layers;

namespace AIResearch.Architecture.Application.Services.Interfaces;

internal interface ICodeAnalyzerService
{
    IEnumerable<LayerDependency> AnalyzeDependencies(Dictionary<string, string> files);
}