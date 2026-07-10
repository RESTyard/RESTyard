using System.Linq;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace RESTyard.HtoSourceGenerators.Test;

/// <summary>
/// Regression guard for generator incrementality (REF-06/GEN-04): all pipeline stages must
/// produce equatable outputs so unchanged HTOs are not re-analyzed and no source is re-emitted
/// when unrelated code is edited.
/// </summary>
public class IncrementalCacheabilityTests
{
    private const string HtoWithControllerResultType = """
        using RESTyard.AspNetCore.Hypermedia;
        using RESTyard.AspNetCore.Hypermedia.Actions;
        using RESTyard.AspNetCore.Hypermedia.Attributes;
        using RESTyard.AspNetCore.WebApi.AttributedRoutes;
        using Microsoft.AspNetCore.Mvc;

        [assembly: HypermediaAssembly]

        namespace TestHtos;

        [HypermediaObject(Title = "QueryResult", Classes = ["QueryResult"])]
        public class HypermediaQueryResultHto : HypermediaObject
        {
            public string ResultData { get; set; } = string.Empty;
        }

        public class CreateQueryAction : HypermediaAction
        {
            public CreateQueryAction() : base(() => true) { }
        }

        [HypermediaObject(Title = "Root", Classes = ["Root"])]
        public class HypermediaRootHto : HypermediaObject
        {
            [HypermediaAction(Name = "CreateQuery")]
            public CreateQueryAction? CreateQuery { get; set; }
        }

        [ApiController]
        [Route("api")]
        public class RootController : ControllerBase
        {
            [HttpPost("query")]
            [HypermediaActionEndpoint<HypermediaRootHto>("CreateQuery",
                ResultType = typeof(HypermediaQueryResultHto))]
            public IActionResult CreateQuery() => Ok();
        }
        """;

    /// <summary>
    /// Runs the generator with step tracking, then re-runs it on the same compilation
    /// plus one unrelated syntax tree, and returns the tracked steps of the second run.
    /// </summary>
    private static GeneratorRunResult RunTwiceWithUnrelatedChange(params string[] sources)
    {
        var compilation = GeneratorTestHelper.CreateCompilation(sources);

        var driver = CSharpGeneratorDriver.Create(
            new[] { new HtoSchemaGenerator().AsSourceGenerator() },
            driverOptions: new GeneratorDriverOptions(
                IncrementalGeneratorOutputKind.None,
                trackIncrementalGeneratorSteps: true));

        var afterFirstRun = driver.RunGenerators(compilation);

        var changedCompilation = compilation.AddSyntaxTrees(
            CSharpSyntaxTree.ParseText(
                "namespace Unrelated { public class NotAnHto { } }",
                GeneratorTestHelper.ParseOptions));

        return afterFirstRun.RunGenerators(changedCompilation).GetRunResult().Results.Single();
    }

    private static void AssertStageIsCached(GeneratorRunResult result, string trackingName)
    {
        var outputs = result.TrackedSteps[trackingName]
            .SelectMany(step => step.Outputs)
            .ToList();

        outputs.Should().NotBeEmpty();
        outputs.Should().OnlyContain(output =>
            output.Reason == IncrementalStepRunReason.Cached
            || output.Reason == IncrementalStepRunReason.Unchanged);
    }

    /// <summary>
    /// Asserts that no RegisterSourceOutput block re-ran, i.e. no source was re-emitted.
    /// This is the end-to-end incrementality guarantee (GEN-04).
    /// </summary>
    private static void AssertOutputsAreCached(GeneratorRunResult result)
    {
        var outputs = result.TrackedOutputSteps
            .SelectMany(kvp => kvp.Value)
            .SelectMany(step => step.Outputs)
            .ToList();

        outputs.Should().NotBeEmpty();
        outputs.Should().OnlyContain(output =>
            output.Reason == IncrementalStepRunReason.Cached
            || output.Reason == IncrementalStepRunReason.Unchanged);
    }

    [Fact]
    public void HtoMetadata_stage_is_cached_when_unrelated_source_is_added()
    {
        var result = RunTwiceWithUnrelatedChange(TestHtoSources.SimpleHto);

        AssertStageIsCached(result, TrackingNames.HtoTypes);
    }

    [Fact]
    public void HtoMetadata_stage_is_cached_for_hto_with_links_actions_and_embedded_entities()
    {
        var result = RunTwiceWithUnrelatedChange(TestHtoSources.FullHto);

        AssertStageIsCached(result, TrackingNames.HtoTypes);
    }

    [Fact]
    public void AssemblyConfig_stage_is_cached_when_unrelated_source_is_added()
    {
        var result = RunTwiceWithUnrelatedChange(TestHtoSources.SimpleHto);

        AssertStageIsCached(result, TrackingNames.AssemblyConfig);
    }

    [Fact]
    public void AssemblyName_stage_is_cached_when_unrelated_source_is_added()
    {
        var result = RunTwiceWithUnrelatedChange(TestHtoSources.SimpleHto);

        AssertStageIsCached(result, TrackingNames.AssemblyName);
    }

    [Fact]
    public void ActionResultMappings_stage_is_cached_when_unrelated_source_is_added()
    {
        var result = RunTwiceWithUnrelatedChange(HtoWithControllerResultType);

        AssertStageIsCached(result, TrackingNames.EndpointResultMappings);
        AssertStageIsCached(result, TrackingNames.ActionResultMappings);
    }

    [Fact]
    public void Outputs_are_cached_when_unrelated_source_is_added()
    {
        var result = RunTwiceWithUnrelatedChange(TestHtoSources.FullHto);

        AssertOutputsAreCached(result);
    }

    [Fact]
    public void Outputs_are_cached_for_hto_with_controller_result_type()
    {
        var result = RunTwiceWithUnrelatedChange(HtoWithControllerResultType);

        AssertOutputsAreCached(result);
    }
}
