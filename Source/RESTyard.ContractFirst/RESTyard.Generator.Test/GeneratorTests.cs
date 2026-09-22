using System.Globalization;
using AwesomeAssertions;

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

    // TODO: verify output syntax for C#
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

    private static IDisposable UseCulture(string name) => UseCulture(new CultureInfo(name));
    
    private static IDisposable UseCulture(CultureInfo cultureInfo)
    {
        var oldCulture = CultureInfo.CurrentCulture;
        var oldUiCulture = CultureInfo.CurrentUICulture;
        var oldDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
        var oldDefaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
        var resetCulture = new ActionDisposable(() =>
        {
            CultureInfo.CurrentCulture = oldCulture;
            CultureInfo.CurrentUICulture = oldUiCulture;
            CultureInfo.DefaultThreadCurrentCulture = oldDefaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = oldDefaultUiCulture;
        });
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        return resetCulture;
    }

    private static string TemplateToNamespace(string template)
    {
        return template.Replace(".", "._").Replace("/", "._").Replace("-", "_");
    }

    private async Task Verify(string file)
        => await VerifyFile(file)
            .UseDirectory("Snapshots");

    private static Task VerifyCompilation(
        string file,
        RestyardVersion version,
        string? additionalCode = null,
        params string[] additionalSources)
        => GeneratedCodeCompiler.VerifyAsync(version, File.ReadAllText(file), additionalCode, additionalSources);

    [Fact]
    public async Task ServerCSharpV4Test()
    {
        await RunGeneratorAsync(
            "server/csharp/v4",
            outputFile: "server_v4.cs",
            includeNamespaces: [AdditionalCodeNamespace]);

        await Verify("server_v4.cs");
        await VerifyCompilation("server_v4.cs", RestyardVersion.LegacyV4, LegacyV4AdditionalCode);
    }

    [Fact]
    public async Task ServerCSharpV5Test()
    {
        await RunGeneratorAsync(
            "server/csharp/v5",
            outputFile: "server_v5.cs",
            includeNamespaces: [AdditionalCodeNamespace, "HypermediaQueryResult = RESTyard.Generator.Test.Output.HypermediaQueryResult_V5_0"]);

        await Verify("server_v5.cs");
        await VerifyCompilation(
            "server_v5.cs",
            RestyardVersion.LegacyV5,
            LegacyV5AdditionalCode);
    }

    [Fact]
    public async Task ServerCSharpV5_1Test()
    {
        await RunGeneratorAsync(
            "server/csharp/v5.1",
            outputFile: "server_v5_1.cs",
            includeNamespaces: [AdditionalCodeNamespace]);

        await Verify("server_v5_1.cs");
        await VerifyCompilation(
            "server_v5_1.cs",
            RestyardVersion.LegacyV5_1,
            CurrentAdditionalCode);
    }

    [Fact]
    public async Task ServerCSharpV5_2Test()
    {
        await RunGeneratorAsync(
            "server/csharp/v5.2",
            outputFile: "server_v5_2.cs",
            includeNamespaces: [AdditionalCodeNamespace]);

        await Verify("server_v5_2.cs");
        await VerifyCompilation(
            "server_v5_2.cs",
            RestyardVersion.Current,
            CurrentAdditionalCode);
    }

    [Fact]
    public async Task ServerCSharpControllerV5Test()
    {
        await RunGeneratorAsync(
            "server/csharp-controller/v5",
            outputFile: "server_controller_v5.cs",
            includeNamespaces: [AdditionalCodeNamespace, TemplateToNamespace("server/csharp/v5.2")]);
        await RunGeneratorAsync(
            "server/csharp/v5.2",
            outputFile: "server_v5_2_for_controller.cs",
            includeNamespaces: [AdditionalCodeNamespace]);

        await Verify("server_controller_v5.cs");
        await VerifyCompilation(
            "server_controller_v5.cs",
            RestyardVersion.Current,
            CurrentAdditionalCode,
            additionalSources:
            [
                File.ReadAllText("server_v5_2_for_controller.cs"),
            ]);
    }

    [Fact]
    public async Task ServerCSharpPoliciesV4Test()
    {
        await RunGeneratorAsync(
            "server/csharp-policies/v4",
            outputFile: "server_policies_v4.cs",
            @namespace: TemplateToNamespace("server/csharp/v4"),
            includeNamespaces: [AdditionalCodeNamespace]);

        await Verify("server_policies_v4.cs");
        await VerifyCompilation("server_policies_v4.cs", RestyardVersion.LegacyV4, LegacyV4AdditionalCode);
    }

    [Fact]
    public async Task ClientCSharpV3Test()
    {
        await RunGeneratorAsync(
            "client/csharp/v3",
            outputFile: "client_v3.cs",
            includeNamespaces: [AdditionalCodeNamespace]);

        await Verify("client_v3.cs");
        await VerifyCompilation("client_v3.cs", RestyardVersion.LegacyV4, LegacyV4AdditionalCode);
    }

    [Fact]
    public async Task ClientTypescriptV0Test()
    {
        await RunGeneratorAsync(
            "client/typescript/v0",
            outputFile: "client_v0.ts");

        await Verify("client_v0.ts");
    }

    [Fact]
    public async Task TurkishLocale()
    {
        /*
         * When running under the turkish locale tr-TR certain characters are
         * not converted to their ASCII counterpart when lowering or upping characters.
         * (e.g. i -> İ)
         */
        
        const string outputFile = "turkish_locale.cs";
        
        using (UseCulture("tr-TR"))
        {
            await RunGeneratorAsync(
                "client/csharp/v3",
                outputFile: outputFile);
        }

        await Verify(outputFile);
    }

    private const string AdditionalCodeNamespace = "RESTyard.Generator.Test.Output";

    private const string LegacyV4AdditionalCode = $$"""
        using RESTyard.AspNetCore.Hypermedia.Actions;

        namespace {{AdditionalCodeNamespace}};

        public record External() : IHypermediaActionParameter;
        """;

    private const string LegacyV5AdditionalCode = $$"""
        using RESTyard.AspNetCore.Hypermedia;
        using RESTyard.AspNetCore.Hypermedia.Actions;
        using RESTyard.AspNetCore.Query;

        namespace {{AdditionalCodeNamespace}};

        public record External : IHypermediaActionParameter;

        public class HypermediaQueryResult_V5_0 : IHypermediaQueryResult
        {
            public IHypermediaQuery Query { get; }
            public HypermediaQueryResult_V5_0(IHypermediaQuery query) => Query = query;
        }
        """;

    private const string CurrentAdditionalCode = $$"""
        using RESTyard.AspNetCore.Hypermedia.Actions;

        namespace {{AdditionalCodeNamespace}};

        public record External() : IHypermediaActionParameter;
        """;

    private const string CurrentControllerAdditionalCode = $$"""
        using RESTyard.AspNetCore.Hypermedia;
        using RESTyard.AspNetCore.Hypermedia.Actions;
        using RESTyard.AspNetCore.Query;

        namespace {{AdditionalCodeNamespace}};

        public record External : IHypermediaActionParameter;

        public class HypermediaQueryResult_V5_0 : IHypermediaQueryResult
        {
            public IHypermediaQuery Query { get; }
            public string? SirenTitle { get; set; }

            public HypermediaQueryResult_V5_0(IHypermediaQuery query) => Query = query;
        }
        """;
}
