using System.Linq;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace RESTyard.HtoSourceGenerators.Test;

/// <summary>
/// Regression guard for generator incrementality (REF-06): the metadata stages must produce
/// equatable outputs so unchanged HTOs are not re-analyzed when unrelated code is edited.
/// NOTE: the combined output stages are NOT asserted yet — the compilation-wide controller
/// scan (GEN-04) recomputes non-equatable results on every compilation change and defeats
/// output-level caching. Extend these tests to the output nodes when GEN-04 is fixed.
/// </summary>
public class IncrementalCacheabilityTests
{
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
}
