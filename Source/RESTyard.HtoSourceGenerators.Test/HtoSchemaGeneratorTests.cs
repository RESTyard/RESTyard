using AwesomeAssertions;
using Xunit;

namespace RESTyard.HtoSourceGenerators.Test;

public class HtoSchemaGeneratorTests
{
    [Fact]
    public void Generator_runs_without_errors()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.SimpleHto);

        result.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Generator_runs_with_full_hto_without_errors()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.FullHto);

        result.Diagnostics.Should().BeEmpty();
    }
}
