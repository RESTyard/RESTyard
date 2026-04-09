using System;
using System.Collections.Generic;
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
    /// generates the requested artifacts and returns <c>true</c>. If <c>--schema-help</c> is present,
    /// prints available arguments to the console and returns <c>true</c>. Otherwise returns <c>false</c>.
    /// </summary>
    /// <param name="schema">The schema to generate artifacts from.</param>
    /// <param name="args">Command-line arguments to parse.</param>
    /// <returns><c>true</c> if schema generation or help was handled; <c>false</c> otherwise.</returns>
    public static bool GenerateIfRequested(HypermediaApiSchema schema, string[] args)
    {
        if (args.Contains("--schema-help", StringComparer.OrdinalIgnoreCase))
        {
            PrintHelp();
            return true;
        }

        if (!args.Contains("--generate-schema", StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        var outputPath = GetArgValue(args, "--schema-output") ?? DefaultOutputPath;
        var artifacts = ParseFormats(GetArgValue(args, "--schema-artifacts"));
        var options = ParseGeneratorOptions(args);

        schema = ApplyAccessGroupFilter(schema, args);

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
            var mermaid = schema.ToApiMap();
            var content = options.MermaidWrapMarkdown
                ? WrapMermaidInMarkdown("# API Map", mermaid)
                : mermaid;
            File.WriteAllText(Path.Combine(outputPath, "api-map.md"), content);
        }

        if (formats.HasFlag(SchemaOutputFormats.MermaidHtos))
        {
            var mermaidOptions = new MermaidMapperOptions
            {
                IncludeProperties = options.MermaidIncludeProperties,
                IncludeActions = options.MermaidIncludeActions,
            };
            var mermaid = schema.ToClassDiagram(mermaidOptions);
            var content = options.MermaidWrapMarkdown
                ? WrapMermaidInMarkdown("# HTOs", mermaid)
                : mermaid;
            File.WriteAllText(Path.Combine(outputPath, "htos.md"), content);
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
        foreach (var part in formatArg!.Split(','))
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

        var wrapMd = GetArgValue(args, "--mermaid-wrap-markdown");
        if (wrapMd != null)
        {
            options.MermaidWrapMarkdown = ParseBool(wrapMd, true);
        }

        return options;
    }

    /// <summary>
    /// Parses <c>--access-groups</c> and <c>--exclude-access-groups</c> CLI arguments
    /// and applies the corresponding filter. Throws if both are specified.
    /// No ISchemaAccessGroupSanitizer is applied — the CLI caller is trusted.
    /// </summary>
    private static HypermediaApiSchema ApplyAccessGroupFilter(HypermediaApiSchema schema, string[] args)
    {
        var includeGroups = GetArgValue(args, "--access-groups");
        var excludeGroups = GetArgValue(args, "--exclude-access-groups");

        if (includeGroups != null && excludeGroups != null)
        {
            throw new InvalidOperationException(
                "Cannot specify both '--access-groups' and '--exclude-access-groups'. Use one or the other.");
        }

        if (includeGroups != null)
        {
            return HypermediaSchemaFilter.ForAccessGroups(schema, ParseCommaSeparatedGroups(includeGroups));
        }

        if (excludeGroups != null)
        {
            return HypermediaSchemaFilter.ExcludeAccessGroups(schema, ParseCommaSeparatedGroups(excludeGroups));
        }

        return schema;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
            RESTyard Schema Generation

            USAGE:
              myapp --generate-schema [options]
              myapp --schema-help

              When using 'dotnet run', separate app args with '--':
                dotnet run -- --generate-schema [options]

            COMMANDS:
              --generate-schema                   Generate schema artifacts and exit
              --schema-help                       Print this help and exit

            OUTPUT:
              --schema-output <path>              Output directory (default: ./generated-schema)
              --schema-artifacts <artifacts>       Comma-separated list (default: all)
                  json-hypermedia-api-schema       Full HypermediaApiSchema as JSON
                  mermaid-api-map                  API map diagram
                  mermaid-htos                     HTO class diagram
                  markdown-api-documentation       Markdown API reference
                  all                              All of the above

            ACCESS GROUP FILTERING:
              --access-groups <groups>             Include mode: comma-separated, keep elements
                                                  visible to any of these groups (OR semantics)
              --exclude-access-groups <groups>     Exclude mode: comma-separated, remove elements
                                                  matching any of these groups
              (mutually exclusive — specify one or neither)

            MAPPER OPTIONS:
              --mermaid-include-properties <bool>  Include properties in HTO diagram (default: true)
              --mermaid-include-actions <bool>     Include actions in HTO diagram (default: true)
              --mermaid-wrap-markdown <bool>       Wrap Mermaid in Markdown fenced block (default: true)
              --markdown-include-toc <bool>        Include table of contents (default: true)
              --markdown-include-diagram <bool>    Include Mermaid diagram in Markdown (default: true)
            """);
    }

    private static HashSet<string> ParseCommaSeparatedGroups(string value)
    {
        return new HashSet<string>(
            value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(g => g.Trim()),
            StringComparer.Ordinal);
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

    private static string WrapMermaidInMarkdown(string title, string mermaid)
    {
        return $"{title}\n\n```mermaid\n{mermaid}\n```\n";
    }

    private static bool ParseBool(string value, bool defaultValue)
    {
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }
}
