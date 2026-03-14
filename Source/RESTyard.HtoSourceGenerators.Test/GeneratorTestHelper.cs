using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Json.Schema;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.JsonSchema;
using RESTyard.Schema.Model;

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
        MetadataReference.CreateFromFile(typeof(EntityTypeSchema).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(JsonSchema).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(IJsonSchemaFactory).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(SchemaHelper).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(JsonDocument).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(EnumMemberAttribute).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(System.Uri).Assembly.Location),
        MetadataReference.CreateFromFile(Path.Combine(AssemblyDirectory, "netstandard.dll")),
    ];

    internal static GeneratorDriverRunResult RunGenerator(params string[] sources)
    {
        var (_, driverResult) = RunGeneratorCore(sources);
        return driverResult;
    }

    /// <summary>
    /// Runs the generator and asserts the combined compilation (input + generated) has no errors.
    /// </summary>
    internal static void AssertOutputCompiles(params string[] sources)
    {
        var (outputCompilation, _) = RunGeneratorCore(sources);

        var errors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToImmutableArray();

        if (!errors.IsEmpty)
        {
            var errorMessages = string.Join("\n", errors.Select(d => d.ToString()));
            throw new System.InvalidOperationException(
                $"Output compilation has errors:\n{errorMessages}");
        }
    }

    private static (Compilation OutputCompilation, GeneratorDriverRunResult DriverResult) RunGeneratorCore(
        string[] sources)
    {
        var syntaxTrees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToArray();

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: syntaxTrees,
            references: References,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable));

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
            compilation, out var outputCompilation, out _);

        return (outputCompilation, driver.GetRunResult());
    }
}
