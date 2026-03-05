namespace AIResearch.Architecture.Application.Services.Interfaces;

public interface IFilePathService
{
    string GetSourceBasePath();

    string GetSolutionFilePath(string solutionName);

    string GetProjectFolder(string solutionName, string layerName, string? featureName = null);

    string GetProjectFilePath(string solutionName, string layerName, string? featureName = null);

    string GetSourceFilePath(string solutionName, string layerName, string fileName, string? featureName = null);
}