using AIResearch.Architecture.Application.Services.Interfaces;

namespace AIResearch.Architecture.Application.Services;

internal sealed class FilePathService(INamespaceService namespaceService) : IFilePathService
{
    private const string SourceBasePath = "src";

    public string GetSourceBasePath() => SourceBasePath;

    public string GetSolutionFilePath(string solutionName) =>
        $"{Path.Combine(SourceBasePath, solutionName)}.sln";

    public string GetProjectFolder(string solutionName, string layerName, string? featureName = null)
    {
        var projectName = namespaceService.BuildNamespace(solutionName, layerName, featureName);

        return Path.Combine(SourceBasePath, projectName);
    }

    public string GetProjectFilePath(string solutionName, string layerName, string? featureName = null)
    {
        var projectFolder = GetProjectFolder(solutionName, layerName, featureName);
        var projectName = namespaceService.BuildNamespace(solutionName, layerName, featureName);

        return $"{Path.Combine(projectFolder, projectName)}.csproj";
    }

    public string GetSourceFilePath(string solutionName, string layerName, string fileName, string? featureName = null)
    {
        var projectFolder = GetProjectFolder(solutionName, layerName, featureName);

        return $"{Path.Combine(projectFolder, fileName)}.cs";
    }
}