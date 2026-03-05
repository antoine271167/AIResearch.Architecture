using AIResearch.Architecture.Application.Models;
using AIResearch.Architecture.Application.Services.Interfaces;
using AIResearch.Architecture.Domain.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace AIResearch.Architecture.Application.Services;

internal sealed class CSharpCodeGeneratorService(
    INamespaceService namespaceService,
    ITypeMetadataResolverService typeMetadataResolver) : ICodeGeneratorService
{
    public string GenerateCode(CodeGenerationModel model)
    {
        var ns = namespaceService.BuildNamespace(model.SolutionName, model.Layer, model.Feature);
        var usingStatements = GenerateUsingStatements(model.Dependencies, model.SolutionName, model.Feature);
        var typeKind = typeMetadataResolver.ResolveTypeKind(model.ComponentRole);
        var accessibility = typeMetadataResolver.ResolveAccessibility(model.ComponentRole);
        var typeDeclaration = GenerateTypeDeclaration(model.Name, typeKind, model.Commands, model.Comments, model.ImplementsInterfaces, accessibility);

        var code = $"""
                     {usingStatements}
                     namespace {ns};

                     {typeDeclaration}
                     """;

        return FormatCode(code);
    }

    private static string GenerateTypeDeclaration(
        string name,
        CSharpTypeKind typeKind,
        IReadOnlyCollection<string> commands,
        string comments,
        IReadOnlyCollection<string> implementsInterfaces,
        string accessibility)
    {
        return typeKind switch
        {
            CSharpTypeKind.Interface => GenerateInterfaceDeclaration(name, commands, comments, implementsInterfaces, accessibility),
            CSharpTypeKind.Record => GenerateRecordDeclaration(name, comments, implementsInterfaces, accessibility),
            CSharpTypeKind.Struct => GenerateStructDeclaration(name, commands, comments, implementsInterfaces, accessibility),
            CSharpTypeKind.RecordStruct => GenerateRecordStructDeclaration(name, comments, implementsInterfaces, accessibility),
            _ => GenerateClassDeclaration(name, commands, comments, implementsInterfaces, accessibility)
        };
    }

    private static string GenerateInterfaceDeclaration(
        string name,
        IReadOnlyCollection<string> commands,
        string comments,
        IReadOnlyCollection<string> implementsInterfaces,
        string accessibility)
    {
        var inheritance = GenerateInheritanceClause(implementsInterfaces);
        var methods = GenerateInterfaceMethods(commands, comments);
        return $$"""
                 {{accessibility}} interface {{name}}{{inheritance}}
                 {
                 {{methods}}
                 }
                 """;
    }

    private static string GenerateRecordDeclaration(string name, string comments, IReadOnlyCollection<string> implementsInterfaces, string accessibility)
    {
        var commentLine = string.IsNullOrWhiteSpace(comments)
            ? string.Empty
            : $"// {comments}{Environment.NewLine}";
        var inheritance = GenerateInheritanceClause(implementsInterfaces);
        return $"{commentLine}{accessibility} record {name}{inheritance};";
    }

    private static string GenerateStructDeclaration(
        string name,
        IReadOnlyCollection<string> commands,
        string comments,
        IReadOnlyCollection<string> implementsInterfaces,
        string accessibility)
    {
        var inheritance = GenerateInheritanceClause(implementsInterfaces);
        var methods = GenerateMethods(commands, comments);
        return $$"""
                 {{accessibility}} struct {{name}}{{inheritance}}
                 {
                 {{methods}}
                 }
                 """;
    }

    private static string GenerateRecordStructDeclaration(string name, string comments, IReadOnlyCollection<string> implementsInterfaces, string accessibility)
    {
        var commentLine = string.IsNullOrWhiteSpace(comments)
            ? string.Empty
            : $"// {comments}{Environment.NewLine}";
        var inheritance = GenerateInheritanceClause(implementsInterfaces);
        return $"{commentLine}{accessibility} record struct {name}{inheritance};";
    }

    private static string GenerateClassDeclaration(
        string name,
        IReadOnlyCollection<string> commands,
        string comments,
        IReadOnlyCollection<string> implementsInterfaces,
        string accessibility)
    {
        var inheritance = GenerateInheritanceClause(implementsInterfaces);
        var methods = GenerateMethods(commands, comments);
        return $$"""
                 {{accessibility}} class {{name}}{{inheritance}}
                 {
                 {{methods}}
                 }
                 """;
    }

    private static string GenerateInheritanceClause(IReadOnlyCollection<string> implementsInterfaces) => 
        implementsInterfaces.Count == 0 ? string.Empty : $" : {string.Join(", ", implementsInterfaces)}";

    private static string GenerateInterfaceMethods(IReadOnlyCollection<string> commands, string comments)
    {
        if (commands.Count == 0)
        {
            return string.IsNullOrWhiteSpace(comments)
                ? "// TODO: define interface members"
                : $"// TODO: define interface members{Environment.NewLine}// {comments}";
        }

        return string.Join(Environment.NewLine + Environment.NewLine, commands.Select(cmd =>
        {
            var commentLine = string.IsNullOrWhiteSpace(comments)
                ? string.Empty
                : $"// {comments}{Environment.NewLine}";
            return $"{commentLine}void HandleAsync({cmd} command);";
        }));
    }

    private static string FormatCode(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        using var workspace = new AdhocWorkspace();
        var formattedRoot = Formatter.Format(root, workspace);

        return formattedRoot.ToFullString();
    }

    private string GenerateUsingStatements(
        IReadOnlyCollection<string> dependencies,
        string solutionName,
        string feature)
    {
        if (dependencies.Count == 0)
        {
            return string.Empty;
        }

        var namespaces = dependencies
            .Select(dep => namespaceService.InferNamespaceForDependency(dep, solutionName, feature))
            .Distinct()
            .OrderBy(ns => ns);

        return string.Join(Environment.NewLine, namespaces.Select(ns => $"using {ns};"));
    }

    private static string GenerateMethods(IReadOnlyCollection<string> commands, string comments)
    {
        if (commands.Count == 0)
        {
            return string.IsNullOrWhiteSpace(comments)
                ? "// TODO: implement class logic"
                : $"// TODO: implement class logic{Environment.NewLine}// {comments}";
        }

        return string.Join(Environment.NewLine + Environment.NewLine, commands.Select(cmd =>
        {
            var commentLine = string.IsNullOrWhiteSpace(comments)
                ? string.Empty
                : $"{Environment.NewLine}// {comments}";

            return
                $"public void HandleAsync({cmd} command){Environment.NewLine}{{{Environment.NewLine}    // TODO: implement logic here{commentLine}{Environment.NewLine}}}";
        }));
    }
}