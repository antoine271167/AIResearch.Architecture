using System.Security.Cryptography;
using System.Text;
using AIResearch.Architecture.Application.Commands.GenerateWorkspace;
using AIResearch.Architecture.Application.Services.Interfaces;
using AIResearch.Architecture.Contracts.Models;
using AIResearch.Architecture.Contracts.Models.Requests;
using AIResearch.Architecture.Domain.Services.Interfaces;

namespace AIResearch.Architecture.Application.Services;

internal sealed class WorkspaceService(
    IFilePathService filePathService,
    ILayerDependencyService layerDependencyService,
    IArchitectureLayersProvider architectureLayersProvider) : IWorkspaceService
{
    private static readonly Guid _cSharpProjectTypeGuid = new("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");
    private static readonly Guid _baseNamespace = new("6BA7B810-9DAD-11D1-80B4-00C04FD430C8");

    public GenerateWorkspaceResult Generate(GenerateWorkspaceRequest request)
    {
        var files = new List<FileContent>();

        // 1. Collect all projects first
        var projectFiles = new List<FileContent>();

        foreach (var feature in request.Features)
        {
            var projects = GetProjectsForFeature(
                request.SolutionName,
                feature,
                request.TargetFramework
            );

            projectFiles.AddRange(projects);
        }


        // 2. Generate solution file with project references
        files.Add(new FileContent
        {
            Path = filePathService.GetSolutionFilePath(request.SolutionName),
            Code = GenerateSolutionFile(request.SolutionName, projectFiles)
        });

        // 3. Add all project files
        files.AddRange(projectFiles);

        return new GenerateWorkspaceResult("ok", files);
    }

    private List<FileContent> GetProjectsForFeature(
        string solutionName,
        FeatureDefinitionDto feature,
        string targetFramework)
    {
        var files = new List<FileContent>();

        var layers = architectureLayersProvider.GetLayers();

        foreach (var layer in layers)
        {
            if (IsHost(layer.Name) && feature.ApplicationKind != ApplicationKind.WebApi)
            {
                continue;
            }

            files.Add(CreateProject(
                filePathService.GetProjectFilePath(solutionName, layer.Name, feature.Name),
                targetFramework,
                GetProjectReferences(solutionName, feature.Name, layer.Name),
                IsWebHost(layer.Name)
            ));
        }

        return files;
    }

    private List<string> GetProjectReferences(string solutionName, string? featureName, string layerName)
    {
        var allowedDependencies = layerDependencyService.GetAllowedDependencies(layerName);

        return allowedDependencies
            .Select(dependency => featureName != null
                ? $"{solutionName}.{featureName}.{dependency}"
                : $"{solutionName}.{dependency}")
            .ToList();
    }

    private static bool IsHost(string layerName) => layerName == nameof(ApplicationKind.WebApi);
    private static bool IsWebHost(string layerName) => layerName == nameof(ApplicationKind.WebApi);

    private static FileContent CreateProject(
        string projectPath,
        string targetFramework,
        List<string> projectRefs,
        bool isWeb = false)
    {
        var sdk = isWeb ? "Microsoft.NET.Sdk.Web" : "Microsoft.NET.Sdk";
        var projectReferences = projectRefs.Count != 0
            ? $"""

                 <ItemGroup>
               {string.Join("\n", projectRefs.Select(r => $"""    <ProjectReference Include="..\{r}\{r}.csproj" />"""))}
                 </ItemGroup>
               """
            : string.Empty;

        var content = $"""
                       <Project Sdk="{sdk}">
                         <PropertyGroup>
                           <TargetFramework>{targetFramework}</TargetFramework>
                           <ImplicitUsings>enable</ImplicitUsings>
                           <Nullable>enable</Nullable>
                         </PropertyGroup>{projectReferences}
                       </Project>
                       """;

        return new FileContent
        {
            Path = projectPath,
            Code = content
        };
    }

    private static string GenerateSolutionFile(
        string solutionName,
        List<FileContent> projectFiles)
    {
        var content = """
                      Microsoft Visual Studio Solution File, Format Version 12.00
                      # Visual Studio Version 17

                      """;

        var projectEntries = new List<string>();
        var projectGuids = new List<Guid>();

        // Generate project entries
        foreach (var projectFile in projectFiles)
        {
            var projectPath = projectFile.Path;
            var projectName = Path.GetFileNameWithoutExtension(projectPath);
            var projectGuid = GenerateProjectGuid(solutionName, projectName);

            // Make path relative to solution file (remove "src/" prefix since .sln is also in src/)
            var relativePath = projectPath.StartsWith("src/") || projectPath.StartsWith("src\\")
                ? projectPath[4..]
                : projectPath;

            projectGuids.Add(projectGuid);

            projectEntries.Add(
                $"Project(\"{{{_cSharpProjectTypeGuid.ToString().ToUpper()}}}\") = \"{projectName}\", \"{relativePath}\", \"{{{projectGuid.ToString().ToUpper()}}}\"");
            projectEntries.Add("EndProject");
        }

        if (projectEntries.Count > 0)
        {
            content += string.Join("\n", projectEntries) + "\n";
        }

        // Add global section
        content += """
                   Global
                   	GlobalSection(SolutionConfigurationPlatforms) = preSolution
                   		Debug|Any CPU = Debug|Any CPU
                   		Release|Any CPU = Release|Any CPU
                   	EndGlobalSection
                   	GlobalSection(ProjectConfigurationPlatforms) = postSolution

                   """;

        // Add project configurations
        foreach (var projectGuid in projectGuids)
        {
            var guidString = $"{{{projectGuid.ToString().ToUpper()}}}";
            content += $"\t\t{guidString}.Debug|Any CPU.ActiveCfg = Debug|Any CPU\n";
            content += $"\t\t{guidString}.Debug|Any CPU.Build.0 = Debug|Any CPU\n";
            content += $"\t\t{guidString}.Release|Any CPU.ActiveCfg = Release|Any CPU\n";
            content += $"\t\t{guidString}.Release|Any CPU.Build.0 = Release|Any CPU\n";
        }

        content += """
                   	EndGlobalSection
                   	GlobalSection(SolutionProperties) = preSolution
                   		HideSolutionNode = FALSE
                   	EndGlobalSection
                   EndGlobal
                   """;

        return content;
    }

    private static Guid GenerateProjectGuid(string solutionName, string projectName)
    {
        var solutionNamespace = GenerateSolutionNamespace(solutionName);
        return GenerateGuidFromNamespace(solutionNamespace, projectName);
    }

    private static Guid GenerateSolutionNamespace(string solutionName) =>
        GenerateGuidFromNamespace(_baseNamespace, solutionName);

    private static Guid GenerateGuidFromNamespace(Guid namespaceGuid, string name)
    {
        using var sha1 = SHA1.Create();
        var namespaceBytes = namespaceGuid.ToByteArray();
        var nameBytes = Encoding.UTF8.GetBytes(name);

        var combinedBytes = CombineBytes(namespaceBytes, nameBytes);
        var hash = sha1.ComputeHash(combinedBytes);

        return CreateGuidFromHash(hash);
    }

    private static byte[] CombineBytes(byte[] first, byte[] second)
    {
        var combined = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, combined, 0, first.Length);
        Buffer.BlockCopy(second, 0, combined, first.Length, second.Length);
        return combined;
    }

    private static Guid CreateGuidFromHash(byte[] hash)
    {
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);

        guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | 0x50);
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

        return new Guid(guidBytes);
    }
}