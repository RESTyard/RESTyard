using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.HtoSourceGenerators;

namespace RESTyard.HtoSourceGenerators.Test;

internal static class GeneratorTestHelper
{
    private static readonly string AssemblyDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

    private static readonly MetadataReference[] References =
    [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
        MetadataReference.CreateFromFile(Path.Combine(AssemblyDirectory, "System.Runtime.dll")),
        MetadataReference.CreateFromFile(Path.Combine(AssemblyDirectory, "System.Collections.dll")),
        MetadataReference.CreateFromFile(typeof(IHypermediaObject).Assembly.Location),
    ];

    internal static GeneratorDriverRunResult RunGenerator(params string[] sources)
    {
        var syntaxTrees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToArray();

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: syntaxTrees,
            references: References,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable));

        // Verify input compilation has no errors (warnings are ok)
        var inputDiagnostics = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToImmutableArray();

        if (!inputDiagnostics.IsEmpty)
        {
            var errors = string.Join("\n", inputDiagnostics.Select(d => d.ToString()));
            throw new System.InvalidOperationException(
                $"Test source has compilation errors:\n{errors}");
        }

        var generator = new HtoSchemaGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation, out _, out _);

        return driver.GetRunResult();
    }
}
