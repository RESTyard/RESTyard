using System.Reflection;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.Client.Hypermedia;

namespace RESTyard.Generator.Test;

public enum RestyardVersion
{
    LegacyV4,
    LegacyV5,
    LegacyV5_1,
    Current,
}

public static class GeneratedCodeCompiler
{
    private const string LegacyV4PackageVersion = "4.4.0-develop-20250414.1";
    private const string LegacyV5PackageVersion = "5.0.0-develop-20250515.1";
    private const string LegacyV5_1PackageVersion = "6.0.2-develop-20260827.3";

    public static async Task VerifyAsync(
        RestyardVersion version,
        string generatedCode,
        string? additionalCode = null,
        params string[] additionalSources)
    {
        using var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId("GeneratedCode");
        var solution = workspace.CurrentSolution
            .AddProject(projectId, "GeneratedCode", "GeneratedCode", LanguageNames.CSharp)
            .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable))
            .WithProjectParseOptions(projectId, new CSharpParseOptions(LanguageVersion.Latest));

        foreach (var reference in GetReferences(version))
        {
            solution = solution.AddMetadataReference(projectId, reference);
        }

        solution = solution.AddDocument(DocumentId.CreateNewId(projectId), "Generated.cs", SourceText.From(generatedCode));
        if (additionalCode is not null)
        {
            solution = solution.AddDocument(DocumentId.CreateNewId(projectId), "Additional.cs", SourceText.From(additionalCode));
        }
        foreach (var (source, index) in additionalSources.Select((source, index) => (source, index)))
        {
            solution = solution.AddDocument(DocumentId.CreateNewId(projectId), $"Dependency{index}.cs", SourceText.From(source));
        }
        solution = solution.AddDocument(
            DocumentId.CreateNewId(projectId),
            "GlobalUsings.cs",
            SourceText.From("""
                global using System;
                global using System.Collections.Generic;
                global using System.Linq;
                global using System.Threading.Tasks;

                [AttributeUsage(AttributeTargets.Class)]
                public sealed class TitleAttribute(string title) : Attribute;
                """));

        var diagnostics = (await solution.GetProject(projectId)!.GetCompilationAsync())!
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .OrderBy(diagnostic => diagnostic.Location.SourceTree?.FilePath)
            .ThenBy(diagnostic => diagnostic.Location.SourceSpan.Start)
            .ToArray();

        diagnostics.Should().BeEmpty(because: string.Join(Environment.NewLine, diagnostics.Select(diagnostic => diagnostic.ToString())));
    }

    private static IEnumerable<MetadataReference> GetReferences(RestyardVersion version)
    {
        var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Append(typeof(IHypermediaObject).Assembly.Location)
            .Append(typeof(HypermediaClientObject).Assembly.Location)
            .Append(typeof(GeneratedCodeCompiler).Assembly.Location);

        if (version is not RestyardVersion.Current)
        {
            paths = paths
                .Where(path => !Path.GetFileName(path).Equals("RESTyard.AspNetCore.dll", StringComparison.OrdinalIgnoreCase))
                .Append(GetLegacyAssemblyPath(version));
        }

        return paths
            .Where(File.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => MetadataReference.CreateFromFile(path));
    }

    private static string GetLegacyAssemblyPath(RestyardVersion version)
    {
        var packageVersion = version switch
        {
            RestyardVersion.LegacyV4 => LegacyV4PackageVersion,
            RestyardVersion.LegacyV5 => LegacyV5PackageVersion,
            RestyardVersion.LegacyV5_1 => LegacyV5_1PackageVersion,
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, ""),
        };
        var packageRoot = Environment.GetEnvironmentVariable("NUGET_PACKAGES")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
        var pathNet8 = Path.Combine(packageRoot, "restyard.aspnetcore", packageVersion, "lib", "net8.0", "RESTyard.AspNetCore.dll");
        var pathNet10 = Path.Combine(packageRoot, "restyard.aspnetcore", packageVersion, "lib", "net10.0", "RESTyard.AspNetCore.dll");
        if (File.Exists(pathNet8))
        {
            return pathNet8;
        }
        else if (File.Exists(pathNet10))
        {
            return pathNet10;
        }
        false.Should().BeTrue($"the RESTyard.AspNetCore package {packageVersion} must be restored");
        return "";
    }
}
