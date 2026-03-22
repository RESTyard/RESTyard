using System;
using System.IO;
using System.Linq;
using RESTyard.Schema.Markdown;
using RESTyard.Schema.Mermaid;
using RESTyard.Schema.Model;

namespace RESTyard.Schema.Generation;

/// <summary>
/// Generates schema output files (JSON, Mermaid diagrams, Markdown documentation)
/// from a <see cref="HypermediaApiSchema"/>.
/// </summary>
/// <remarks>
/// This class contains pure file-generation logic.
/// It can be used directly by CLI tools and CI pipelines.
/// For ASP.NET Core convenience, use the <c>GenerateSchemaIfRequested</c> extension method on <c>IHost</c>.
/// </remarks>
public static class HypermediaSchemaGenerator
{
    private const string DefaultOutputPath = "./generated-schema";

    /// <summary>
    /// Parses CLI arguments for schema generation flags. If <c>--generate-schema</c> is present,
    /// generates the requested artifacts and returns <c>true</c>. Otherwise returns <c>false</c>.
    /// </summary>
    /// <param name="schema">The schema to generate artifacts from.</param>
    /// <param name="args">Command-line arguments to parse.</param>
    /// <returns><c>true</c> if <c>--generate-schema</c> was present and generation was performed; <c>false</c> otherwise.</returns>
    public static bool GenerateIfRequested(HypermediaApiSchema schema, string[] args)
    {
        if (!args.Contains("--generate-schema", StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        var outputPath = GetArgValue(args, "--schema-output") ?? DefaultOutputPath;
        var artifacts = ParseFormats(GetArgValue(args, "--schema-artifacts"));
        var options = ParseGeneratorOptions(args);

        Generate(schema, outputPath, artifacts, options);
        return true;
    }

    /// <summary>
    /// Generates schema output files in the specified formats.
    /// </summary>
    /// <param name="schema">The schema to generate artifacts from.</param>
    /// <param name="outputPath">Directory path for output files. Created if it doesn't exist.</param>
    /// <param name="formats">Which output formats to generate.</param>
    /// <param name="options">Optional mapper options. When null, all defaults apply.</param>
    public static void Generate(
        HypermediaApiSchema schema,
        string outputPath,
        SchemaOutputFormats formats = SchemaOutputFormats.All,
        SchemaGeneratorOptions? options = null)
    {
        options ??= new SchemaGeneratorOptions();
        Directory.CreateDirectory(outputPath);

        if (formats.HasFlag(SchemaOutputFormats.JsonHypermediaApiSchema))
        {
            File.WriteAllText(Path.Combine(outputPath, "hypermedia-api-schema.json"), schema.ToJson());
        }

        if (formats.HasFlag(SchemaOutputFormats.MermaidApiMap))
        {
            File.WriteAllText(Path.Combine(outputPath, "api-map.md"), schema.ToApiMap());
        }

        if (formats.HasFlag(SchemaOutputFormats.MermaidHtos))
        {
            var mermaidOptions = new MermaidMapperOptions
            {
                IncludeProperties = options.MermaidIncludeProperties,
                IncludeActions = options.MermaidIncludeActions,
            };
            File.WriteAllText(Path.Combine(outputPath, "htos.md"), schema.ToClassDiagram(mermaidOptions));
        }

        if (formats.HasFlag(SchemaOutputFormats.MarkdownApiDocumentation))
        {
            var markdownOptions = new MarkdownMapperOptions
            {
                IncludeTableOfContents = options.MarkdownIncludeToc,
                IncludeDiagram = options.MarkdownIncludeDiagram,
            };
            File.WriteAllText(Path.Combine(outputPath, "api-documentation.md"), schema.ToDocumentation(markdownOptions));
        }
    }

    /// <summary>
    /// Parses a comma-separated format string into <see cref="SchemaOutputFormats"/>.
    /// Returns <see cref="SchemaOutputFormats.All"/> for null, empty, or unrecognized input.
    /// </summary>
    public static SchemaOutputFormats ParseFormats(string? formatArg)
    {
        if (string.IsNullOrWhiteSpace(formatArg))
        {
            return SchemaOutputFormats.All;
        }

        var result = (SchemaOutputFormats)0;
        foreach (var part in formatArg.Split(','))
        {
            var trimmed = part.Trim().ToLowerInvariant();
            result |= trimmed switch
            {
                "json-hypermedia-api-schema" => SchemaOutputFormats.JsonHypermediaApiSchema,
                "mermaid-api-map" => SchemaOutputFormats.MermaidApiMap,
                "mermaid-htos" => SchemaOutputFormats.MermaidHtos,
                "markdown-api-documentation" => SchemaOutputFormats.MarkdownApiDocumentation,
                "all" => SchemaOutputFormats.All,
                _ => 0,
            };
        }

        return result == 0 ? SchemaOutputFormats.All : result;
    }

    /// <summary>
    /// Parses CLI arguments for mapper-specific options (Mermaid properties/actions, Markdown TOC/diagram).
    /// </summary>
    public static SchemaGeneratorOptions ParseGeneratorOptions(string[] args)
    {
        var options = new SchemaGeneratorOptions();

        var mermaidProps = GetArgValue(args, "--mermaid-include-properties");
        if (mermaidProps != null)
        {
            options.MermaidIncludeProperties = ParseBool(mermaidProps, true);
        }

        var mermaidActions = GetArgValue(args, "--mermaid-include-actions");
        if (mermaidActions != null)
        {
            options.MermaidIncludeActions = ParseBool(mermaidActions, true);
        }

        var mdToc = GetArgValue(args, "--markdown-include-toc");
        if (mdToc != null)
        {
            options.MarkdownIncludeToc = ParseBool(mdToc, true);
        }

        var mdDiagram = GetArgValue(args, "--markdown-include-diagram");
        if (mdDiagram != null)
        {
            options.MarkdownIncludeDiagram = ParseBool(mdDiagram, true);
        }

        return options;
    }

    private static string? GetArgValue(string[] args, string key)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }

    private static bool ParseBool(string value, bool defaultValue)
    {
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }
}
