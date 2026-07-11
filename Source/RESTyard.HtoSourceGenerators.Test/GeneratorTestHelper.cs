using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using Json.Schema;
using Json.Schema.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using RESTyard.Schema.SchemaGeneration;
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
        MetadataReference.CreateFromFile(typeof(JsonDocument).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(EnumMemberAttribute).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(TitleAttribute).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(System.Uri).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(ControllerBase).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Microsoft.AspNetCore.Mvc.IActionResult).Assembly.Location),
        MetadataReference.CreateFromFile(Path.Combine(AssemblyDirectory, "netstandard.dll")),
        MetadataReference.CreateFromFile(typeof(FunicularSwitch.Option).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(FunicularSwitch.Result).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(System.Collections.Immutable.ImmutableList<>).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Microsoft.AspNetCore.Http.HttpContext).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(RESTyard.MediaTypes.DefaultMediaTypes).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(System.IServiceProvider).Assembly.Location),
    ];

    internal static GeneratorDriverRunResult RunGenerator(params string[] sources)
    {
        var (_, driverResult) = RunGeneratorCore(sources);
        return driverResult;
    }

    /// <summary>
    /// Runs the generator on sources compiled with additional metadata references —
    /// for multi-assembly scenarios (e.g. a controller-only assembly referencing an HTO assembly).
    /// </summary>
    internal static GeneratorDriverRunResult RunGenerator(
        MetadataReference[] extraReferences, params string[] sources)
    {
        var (_, driverResult) = RunGeneratorCore(sources, extraReferences);
        return driverResult;
    }

    /// <summary>
    /// Compiles sources into a separate assembly (without running the generator) and returns
    /// a metadata reference to it — used to simulate a referenced HTO assembly.
    /// </summary>
    internal static MetadataReference CompileToMetadataReference(string assemblyName, params string[] sources)
    {
        var syntaxTrees = sources.Select(s => CSharpSyntaxTree.ParseText(s, ParseOptions)).ToArray();

        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees,
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable));

        using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms);
        if (!emitResult.Success)
        {
            var errors = string.Join("\n", emitResult.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()));
            throw new InvalidOperationException($"Referenced assembly emit failed:\n{errors}");
        }

        return MetadataReference.CreateFromImage(ms.ToArray());
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

    /// <summary>
    /// Runs the generator, compiles the output, loads the assembly, and invokes
    /// the generated GetSchema method via reflection to return the EntityTypeSchema.
    /// </summary>
    internal static EntityTypeSchema RunGeneratorAndGetSchema(string htoClassName, params string[] sources)
    {
        var (outputCompilation, _) = RunGeneratorCore(sources);

        using var ms = new MemoryStream();
        var emitResult = outputCompilation.Emit(ms);
        if (!emitResult.Success)
        {
            var errors = string.Join("\n", emitResult.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()));
            throw new InvalidOperationException($"Emit failed:\n{errors}");
        }

        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());

        var mapperTypeName = $"TestHtos.{htoClassName}Schema";
        var mapperType = assembly.GetType(mapperTypeName)
                         ?? throw new InvalidOperationException($"Type '{mapperTypeName}' not found in emitted assembly");

        var getSchemaMethod = mapperType.GetMethod("GetSchema", BindingFlags.Public | BindingFlags.Static)
                              ?? throw new InvalidOperationException($"Method 'GetSchema' not found on '{mapperTypeName}'");

        var parameters = getSchemaMethod.GetParameters();
        object? result;
        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IJsonSchemaFactory))
        {
            result = getSchemaMethod.Invoke(null, [new JsonSchemaFactory()]);
        }
        else
        {
            result = getSchemaMethod.Invoke(null, []);
        }

        return (EntityTypeSchema)(result ?? throw new InvalidOperationException("GetSchema returned null"));
    }

    /// <summary>
    /// Runs the generator, compiles the output, loads the assembly, creates an HTO instance,
    /// and invokes the generated ToSiren() method via reflection.
    /// Returns the serialized JSON string of the SirenEntity result.
    /// </summary>
    internal static string RunGeneratorAndGetSirenJson(
        string htoClassName,
        IHypermediaRouteResolver resolver,
        Action<object>? configureHto = null,
        RESTyard.AspNetCore.Hypermedia.Siren.SirenMapperOptions? options = null,
        params string[] sources)
    {
        var (outputCompilation, _) = RunGeneratorCore(sources);

        using var ms = new MemoryStream();
        var emitResult = outputCompilation.Emit(ms);
        if (!emitResult.Success)
        {
            var errors = string.Join("\n", emitResult.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()));
            throw new InvalidOperationException($"Emit failed:\n{errors}");
        }

        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());

        // Create HTO instance
        var htoTypeName = $"TestHtos.{htoClassName}";
        var htoType = assembly.GetType(htoTypeName)
                      ?? throw new InvalidOperationException($"Type '{htoTypeName}' not found in emitted assembly");
        var hto = Activator.CreateInstance(htoType)
                  ?? throw new InvalidOperationException($"Could not create instance of '{htoTypeName}'");
        configureHto?.Invoke(hto);

        // Find and invoke ToSiren()
        var extensionsTypeName = $"TestHtos.{htoClassName}SirenExtensions";
        var extensionsType = assembly.GetType(extensionsTypeName)
                             ?? throw new InvalidOperationException($"Type '{extensionsTypeName}' not found in emitted assembly");
        var toSirenMethod = extensionsType.GetMethod("ToSiren", BindingFlags.Public | BindingFlags.Static)
                            ?? throw new InvalidOperationException($"Method 'ToSiren' not found on '{extensionsTypeName}'");

        var queryStringBuilder = new RESTyard.AspNetCore.Query.QueryStringBuilder();
        var sirenResult = toSirenMethod.Invoke(null, [hto, resolver, queryStringBuilder, options]);
        if (sirenResult == null)
        {
            throw new InvalidOperationException("ToSiren returned null");
        }

        // No PropertyNamingPolicy — the Siren POCOs use explicit [JsonPropertyName] for Siren
        // structural properties (class, title, etc.), and the properties POCO uses the property
        // names from the HTO (PascalCase by default, or [HypermediaProperty(Name)] overrides).
        // This matches SirenConverter behavior which uses PascalCase property names.
        return JsonSerializer.Serialize(sirenResult, sirenResult.GetType(), new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
        });
    }

    /// <summary>
    /// Normalizes JSON for comparison — parse and re-serialize with consistent formatting.
    /// Eliminates whitespace/formatting differences between Newtonsoft and System.Text.Json output.
    /// </summary>
    internal static string NormalizeJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Runs the generator, compiles the output, and returns the loaded assembly.
    /// Useful for creating HTO instances for SirenConverter parity tests.
    /// </summary>
    internal static Assembly EmitAssembly(params string[] sources)
    {
        var (outputCompilation, _) = RunGeneratorCore(sources);

        using var ms = new MemoryStream();
        var emitResult = outputCompilation.Emit(ms);
        if (!emitResult.Success)
        {
            var errors = string.Join("\n", emitResult.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()));
            throw new InvalidOperationException($"Emit failed:\n{errors}");
        }

        ms.Seek(0, SeekOrigin.Begin);
        return Assembly.Load(ms.ToArray());
    }

    internal static readonly CSharpParseOptions ParseOptions =
        new(documentationMode: DocumentationMode.Diagnose);

    /// <summary>
    /// Creates the input compilation from the given sources and asserts it has no errors.
    /// </summary>
    internal static CSharpCompilation CreateCompilation(params string[] sources)
        => CreateCompilation(sources, extraReferences: null);

    private static CSharpCompilation CreateCompilation(string[] sources, MetadataReference[]? extraReferences)
    {
        var syntaxTrees = sources.Select(s => CSharpSyntaxTree.ParseText(s, ParseOptions)).ToArray();

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: syntaxTrees,
            references: extraReferences == null ? References : References.Concat(extraReferences),
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

        return compilation;
    }

    private static (Compilation OutputCompilation, GeneratorDriverRunResult DriverResult) RunGeneratorCore(
        string[] sources, MetadataReference[]? extraReferences = null)
    {
        var compilation = CreateCompilation(sources, extraReferences);

        var generator = new HtoSchemaGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation, out var outputCompilation, out _);

        return (outputCompilation, driver.GetRunResult());
    }
}
