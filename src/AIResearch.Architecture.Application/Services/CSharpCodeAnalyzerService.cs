using AIResearch.Architecture.Application.Services.Interfaces;
using AIResearch.Architecture.Domain.Models.Layers;
using AIResearch.Architecture.Domain.Services.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AIResearch.Architecture.Application.Services;

internal sealed class CSharpCodeAnalyzerService(
    IDependencyAnalysisService dependencyAnalysisService,
    IArchitectureLayersProvider architectureLayersProvider) : ICodeAnalyzerService
{
    public IEnumerable<LayerDependency> AnalyzeDependencies(Dictionary<string, string> files)
    {
        var layers = InitializeLayersDictionary();

        foreach (var file in files)
        {
            ProcessFileForDependencies(file.Value, layers);
        }

        return dependencyAnalysisService.AggregateDependencies(layers);
    }

    private Dictionary<string, HashSet<string>> InitializeLayersDictionary()
    {
        var layerNames = architectureLayersProvider.GetLayers()
            .Select(l => l.Name);

        return layerNames.ToDictionary(
            name => name, _ => new HashSet<string>()
        );
    }

    private void ProcessFileForDependencies(string fileContent, Dictionary<string, HashSet<string>> layers)
    {
        var root = CSharpSyntaxTree.ParseText(fileContent).GetRoot();

        var ns = ExtractNamespace(root);
        if (ns is null)
        {
            return;
        }

        var layer = dependencyAnalysisService.DetermineLayer(ns);
        if (layer == "Unknown")
        {
            return;
        }

        var usings = ExtractUsings(root);
        AddLayerDependencies(layer, usings, layers);
    }

    private static string? ExtractNamespace(SyntaxNode root) =>
        root.DescendantNodes()
            .OfType<NamespaceDeclarationSyntax>()
            .FirstOrDefault()?.Name.ToString();

    private static IEnumerable<string> ExtractUsings(SyntaxNode root)
    {
        return root.DescendantNodes()
            .OfType<UsingDirectiveSyntax>()
            .Select(u => u.Name!.ToString());
    }

    private void AddLayerDependencies(string layer, IEnumerable<string> usings,
        Dictionary<string, HashSet<string>> layers)
    {
        foreach (var usingDirective in usings)
        {
            var referencedLayer = dependencyAnalysisService.DetermineReferencedLayer(usingDirective);
            if (referencedLayer != "Unknown")
            {
                layers[layer].Add(referencedLayer);
            }
        }
    }
}