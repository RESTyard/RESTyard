using System;
using System.IO;
using AwesomeAssertions;
using RESTyard.Schema.Generation;
using RESTyard.Schema.Model;
using Xunit;

namespace RESTyard.Schema.Test.Generation;

public class HypermediaSchemaGeneratorTests : IDisposable
{
    private readonly string _outputPath;
    private readonly HypermediaApiSchema _schema;

    public HypermediaSchemaGeneratorTests()
    {
        _outputPath = Path.Combine(Path.GetTempPath(), $"restyard-test-{Guid.NewGuid():N}");
        _schema = new HypermediaApiSchema
        {
            SchemaVersion = "1.0.0",
            Title = "Test API",
            EntryPointName = "Root",
            EntityTypes =
            [
                new EntityTypeSchema { Name = "Root", Classes = ["EntryPoint"] },
                new EntityTypeSchema { Name = "Customer", Classes = ["Customer"] },
            ],
        };
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputPath))
        {
            Directory.Delete(_outputPath, true);
        }
    }

    [Fact]
    public void GenerateIfRequested_returns_false_when_no_flag()
    {
        var result = HypermediaSchemaGenerator.GenerateIfRequested(_schema, ["--other-arg"]);

        result.Should().BeFalse();
        Directory.Exists(_outputPath).Should().BeFalse();
    }

    [Fact]
    public void GenerateIfRequested_returns_true_for_schema_help()
    {
        var result = HypermediaSchemaGenerator.GenerateIfRequested(_schema, ["--schema-help"]);

        result.Should().BeTrue();
        Directory.Exists(_outputPath).Should().BeFalse("help should not generate files");
    }

    [Fact]
    public void GenerateIfRequested_returns_true_and_generates_all_artifacts()
    {
        var result = HypermediaSchemaGenerator.GenerateIfRequested(
            _schema, ["--generate-schema", "--schema-output", _outputPath]);

        result.Should().BeTrue();
        File.Exists(Path.Combine(_outputPath, "hypermedia-api-schema.json")).Should().BeTrue();
        File.Exists(Path.Combine(_outputPath, "api-map.md")).Should().BeTrue();
        File.Exists(Path.Combine(_outputPath, "htos.md")).Should().BeTrue();
        File.Exists(Path.Combine(_outputPath, "api-documentation.md")).Should().BeTrue();
    }

    [Fact]
    public void GenerateIfRequested_uses_specified_output_path()
    {
        var result = HypermediaSchemaGenerator.GenerateIfRequested(
            _schema, ["--generate-schema", "--schema-output", _outputPath]);

        result.Should().BeTrue();
    }

    [Fact]
    public void Generate_json_only()
    {
        HypermediaSchemaGenerator.Generate(_schema, _outputPath, SchemaOutputFormats.JsonHypermediaApiSchema);

        File.Exists(Path.Combine(_outputPath, "hypermedia-api-schema.json")).Should().BeTrue();
        File.Exists(Path.Combine(_outputPath, "api-map.md")).Should().BeFalse();
        File.Exists(Path.Combine(_outputPath, "htos.md")).Should().BeFalse();
        File.Exists(Path.Combine(_outputPath, "api-documentation.md")).Should().BeFalse();
    }

    [Fact]
    public void Generate_mermaid_artifacts_only()
    {
        HypermediaSchemaGenerator.Generate(
            _schema, _outputPath, SchemaOutputFormats.MermaidApiMap | SchemaOutputFormats.MermaidHtos);

        File.Exists(Path.Combine(_outputPath, "hypermedia-api-schema.json")).Should().BeFalse();
        File.Exists(Path.Combine(_outputPath, "api-map.md")).Should().BeTrue();
        File.Exists(Path.Combine(_outputPath, "htos.md")).Should().BeTrue();
        File.Exists(Path.Combine(_outputPath, "api-documentation.md")).Should().BeFalse();
    }

    [Fact]
    public void Generate_json_contains_schema_content()
    {
        HypermediaSchemaGenerator.Generate(_schema, _outputPath, SchemaOutputFormats.JsonHypermediaApiSchema);

        var json = File.ReadAllText(Path.Combine(_outputPath, "hypermedia-api-schema.json"));
        json.Should().Contain("\"title\"");
        json.Should().Contain("Test API");
        json.Should().Contain("Customer");
    }

    [Fact]
    public void GenerateIfRequested_parses_artifact_selection()
    {
        HypermediaSchemaGenerator.GenerateIfRequested(
            _schema, ["--generate-schema", "--schema-output", _outputPath,
                "--schema-artifacts", "json-hypermedia-api-schema,markdown-api-documentation"]);

        File.Exists(Path.Combine(_outputPath, "hypermedia-api-schema.json")).Should().BeTrue();
        File.Exists(Path.Combine(_outputPath, "api-documentation.md")).Should().BeTrue();
        File.Exists(Path.Combine(_outputPath, "api-map.md")).Should().BeFalse();
        File.Exists(Path.Combine(_outputPath, "htos.md")).Should().BeFalse();
    }

    [Fact]
    public void ParseFormats_returns_all_for_null()
    {
        HypermediaSchemaGenerator.ParseFormats(null).Should().Be(SchemaOutputFormats.All);
    }

    [Fact]
    public void ParseFormats_returns_all_for_empty()
    {
        HypermediaSchemaGenerator.ParseFormats("").Should().Be(SchemaOutputFormats.All);
    }

    [Fact]
    public void ParseFormats_parses_comma_separated()
    {
        HypermediaSchemaGenerator.ParseFormats("json-hypermedia-api-schema,mermaid-api-map").Should()
            .Be(SchemaOutputFormats.JsonHypermediaApiSchema | SchemaOutputFormats.MermaidApiMap);
    }

    [Fact]
    public void ParseFormats_ignores_unknown_values()
    {
        HypermediaSchemaGenerator.ParseFormats("json-hypermedia-api-schema,unknown,markdown-api-documentation").Should()
            .Be(SchemaOutputFormats.JsonHypermediaApiSchema | SchemaOutputFormats.MarkdownApiDocumentation);
    }

    [Fact]
    public void ParseGeneratorOptions_reads_mermaid_options()
    {
        var options = HypermediaSchemaGenerator.ParseGeneratorOptions(
            ["--mermaid-include-properties", "false", "--mermaid-include-actions", "false"]);

        options.MermaidIncludeProperties.Should().BeFalse();
        options.MermaidIncludeActions.Should().BeFalse();
    }

    [Fact]
    public void ParseGeneratorOptions_reads_markdown_options()
    {
        var options = HypermediaSchemaGenerator.ParseGeneratorOptions(
            ["--markdown-include-toc", "false", "--markdown-include-diagram", "false"]);

        options.MarkdownIncludeToc.Should().BeFalse();
        options.MarkdownIncludeDiagram.Should().BeFalse();
    }

    [Fact]
    public void ParseGeneratorOptions_defaults_when_not_specified()
    {
        var options = HypermediaSchemaGenerator.ParseGeneratorOptions([]);

        options.MermaidIncludeProperties.Should().BeTrue();
        options.MermaidIncludeActions.Should().BeTrue();
        options.MarkdownIncludeToc.Should().BeTrue();
        options.MarkdownIncludeDiagram.Should().BeTrue();
        options.MermaidWrapMarkdown.Should().BeTrue();
    }

    [Fact]
    public void Mermaid_files_are_wrapped_in_markdown_by_default()
    {
        HypermediaSchemaGenerator.Generate(
            _schema, _outputPath, SchemaOutputFormats.MermaidApiMap | SchemaOutputFormats.MermaidHtos);

        var apiMap = File.ReadAllText(Path.Combine(_outputPath, "api-map.md"));
        apiMap.Should().StartWith("# API Map");
        apiMap.Should().Contain("```mermaid");
        apiMap.Should().Contain("```");

        var htos = File.ReadAllText(Path.Combine(_outputPath, "htos.md"));
        htos.Should().StartWith("# HTOs");
        htos.Should().Contain("```mermaid");
    }

    [Fact]
    public void Mermaid_files_are_raw_when_wrap_disabled()
    {
        var options = new SchemaGeneratorOptions { MermaidWrapMarkdown = false };
        HypermediaSchemaGenerator.Generate(
            _schema, _outputPath, SchemaOutputFormats.MermaidApiMap, options);

        var apiMap = File.ReadAllText(Path.Combine(_outputPath, "api-map.md"));
        apiMap.Should().NotContain("# API Map");
        apiMap.Should().NotContain("```mermaid");
    }

    [Fact]
    public void GenerateIfRequested_with_access_groups_filters_schema()
    {
        var schema = CreateSchemaWithAccessGroups();

        HypermediaSchemaGenerator.GenerateIfRequested(
            schema, ["--generate-schema", "--schema-output", _outputPath,
                "--schema-artifacts", "json-hypermedia-api-schema",
                "--access-groups", "read"]);

        var json = File.ReadAllText(Path.Combine(_outputPath, "hypermedia-api-schema.json"));
        json.Should().Contain("Root");
        json.Should().Contain("Customer");
        json.Should().NotContain("AdminDashboard");
    }

    [Fact]
    public void GenerateIfRequested_with_exclude_access_groups_filters_schema()
    {
        var schema = CreateSchemaWithAccessGroups();

        HypermediaSchemaGenerator.GenerateIfRequested(
            schema, ["--generate-schema", "--schema-output", _outputPath,
                "--schema-artifacts", "json-hypermedia-api-schema",
                "--exclude-access-groups", "admin"]);

        var json = File.ReadAllText(Path.Combine(_outputPath, "hypermedia-api-schema.json"));
        json.Should().Contain("Customer");
        json.Should().NotContain("AdminDashboard");
    }

    [Fact]
    public void GenerateIfRequested_with_both_access_group_args_throws()
    {
        var schema = CreateSchemaWithAccessGroups();

        var act = () => HypermediaSchemaGenerator.GenerateIfRequested(
            schema, ["--generate-schema", "--schema-output", _outputPath,
                "--access-groups", "read",
                "--exclude-access-groups", "admin"]);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*both*");
    }

    [Fact]
    public void GenerateIfRequested_without_access_groups_keeps_all()
    {
        var schema = CreateSchemaWithAccessGroups();

        HypermediaSchemaGenerator.GenerateIfRequested(
            schema, ["--generate-schema", "--schema-output", _outputPath,
                "--schema-artifacts", "json-hypermedia-api-schema"]);

        var json = File.ReadAllText(Path.Combine(_outputPath, "hypermedia-api-schema.json"));
        json.Should().Contain("Customer");
        json.Should().Contain("AdminDashboard");
        json.Should().Contain("declaredAccessGroups");
    }

    private static HypermediaApiSchema CreateSchemaWithAccessGroups()
    {
        return new HypermediaApiSchema
        {
            SchemaVersion = "1.0.0",
            Title = "Test API",
            EntryPointName = "Root",
            DeclaredAccessGroups = ["admin", "read"],
            EntityTypes =
            [
                new EntityTypeSchema
                {
                    Name = "Root",
                    Classes = ["EntryPoint"],
                    Links =
                    [
                        new LinkDescription { Relations = ["customers"], TargetName = "Customer" },
                        new LinkDescription { Relations = ["admin"], TargetName = "AdminDashboard", AccessGroups = ["admin"] },
                    ],
                },
                new EntityTypeSchema { Name = "Customer", Classes = ["Customer"], AccessGroups = ["read"] },
                new EntityTypeSchema { Name = "AdminDashboard", Classes = ["AdminDashboard"], AccessGroups = ["admin"] },
            ],
        };
    }
}
