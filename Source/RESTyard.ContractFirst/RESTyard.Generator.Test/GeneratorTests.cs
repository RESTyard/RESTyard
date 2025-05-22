using FluentAssertions;

namespace RESTyard.Generator.Test;

public class GeneratorTests
{
    private ITestOutputHelper outputHelper;

    public GeneratorTests(ITestOutputHelper outputHelper)
    {
        this.outputHelper = outputHelper;
    }

    [Fact]
    public Task RunChecks() => VerifyChecks.Run();

    private async Task RunGeneratorAsync(
        string template,
        string outputFile,
        string schemaFile = "TestSchema.xml",
        string? @namespace = null,
        IList<string>? includeNamespaces = null,
        IEnumerable<string>? includeType = null,
        IEnumerable<string>? excludeType = null)
    {
        includeNamespaces ??= [];
        if (template.Contains("csharp"))
        {
            includeNamespaces.Add("RESTyard.Generator.Test.Output");
        }

        var templateNormalized = TemplateToNamespace(template);
        var includeFile = includeNamespaces.Any() ? $"Include_{templateNormalized}.txt" : null;
        if (includeNamespaces.Any())
        {
            var content = string.Join(Environment.NewLine, includeNamespaces.Select(ns => $"using {ns};"));
            await File.WriteAllTextAsync(includeFile!, content);
        }
        var args = Filter([
            ("--schema-file", schemaFile),
            ("--template", template),
            ("--output-file", outputFile),
            ("--namespace", @namespace ?? templateNormalized),
            ("--include-file", includeFile),
            ("--include-type", FormatList(includeType)),
            ("--exclude-type", FormatList(excludeType)),
        ]);
        var stdOut = Console.Out;
        var stdErr = Console.Error;
        await using var newOut = new StringWriter();
        await using var newErr = new StringWriter();
        Console.SetOut(newOut);
        Console.SetError(newErr);
        var result = await RESTyard.Generator.Program.Main(args.ToArray());
        Console.SetOut(stdOut);
        Console.SetError(stdErr);
        this.outputHelper.WriteLine($"""
                                     Output: {newOut}

                                     Error: {newErr}
                                     """);
        result.Should().Be(0);
        return;

        IEnumerable<string> Filter(IEnumerable<(string, string?)> pairs)
        {
            foreach (var (command, value) in pairs)
            {
                if (value is not null)
                {
                    yield return command;
                    yield return value;
                }
            }
        }

        string? FormatList(IEnumerable<string>? list) => list is null ? null : string.Join(",", list);
    }

    private static string TemplateToNamespace(string template)
    {
        return template.Replace(".", "._").Replace("/", "._").Replace("-", "_");
    }

    /// <summary>
    /// Generate into different project such that if the output does not compile, the test can still be executed.
    /// </summary>
    /// <param name="file"></param>
    private async Task Verify(string file, string outputSuffix = "")
        => await VerifyFile(file)
            .UseDirectory($"../RESTyard.Generator.Test.Output{outputSuffix}");

    [Fact]
    public async Task ServerCSharpV4Test()
    {
        await RunGeneratorAsync(
            "server/csharp/v4",
            outputFile: "server_v4.cs");

        await Verify("server_v4.cs");
    }

    [Fact]
    public async Task ServerCSharpV5Test()
    {
        await RunGeneratorAsync(
            "server/csharp/v5",
            outputFile: "server_v5.cs");

        await Verify("server_v5.cs", outputSuffix: "V5");
    }

    [Fact]
    public async Task ServerCSharpControllerV4Test()
    {
        await RunGeneratorAsync(
            "server/csharp-controller/v4",
            outputFile: "server_controller_v4.cs",
            includeNamespaces: [TemplateToNamespace("server/csharp/v4")]);

        await Verify("server_controller_v4.cs");
    }

    [Fact]
    public async Task ServerCSharpPoliciesV4Test()
    {
        await RunGeneratorAsync(
            "server/csharp-policies/v4",
            outputFile: "server_policies_v4.cs",
            @namespace: TemplateToNamespace("server/csharp/v4"));

        await Verify("server_policies_v4.cs");
    }

    [Fact]
    public async Task ClientCSharpV3Test()
    {
        await RunGeneratorAsync(
            "client/csharp/v3",
            outputFile: "client_v3.cs");

        await Verify("client_v3.cs");
    }

    [Fact]
    public async Task ClientTypescriptV0Test()
    {
        await RunGeneratorAsync(
            "client/typescript/v0",
            outputFile: "client_v0.ts");

        await Verify("client_v0.ts");
    }
}