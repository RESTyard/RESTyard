using AwesomeAssertions;
using RESTyard.Client.Reader;

namespace Extensions.Test.ProblemStringReaderTests;

public abstract class ProblemStringReaderTestBase
{
    protected string ProblemString { get; set; }

    protected IProblemStringReader ProblemReader { get; set; }

    [Fact]
    public void ProblemStringReaderTest()
    {
        var canRead = ProblemReader.TryReadProblemString(ProblemString, out var description);
        canRead.Should().BeTrue();
        description!.Title.Should().Be("SomeProblem");
        description.Type.Should().Be("UnitTestProblem");
        description.Detail.Should().Be("This Unit Test was unexpectedly green");
        description.Status.Should().Be(42);
    }
}