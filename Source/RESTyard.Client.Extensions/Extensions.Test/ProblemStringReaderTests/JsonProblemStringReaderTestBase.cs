namespace Extensions.Test.ProblemStringReaderTests;

public abstract class JsonProblemStringReaderTestBase : ProblemStringReaderTestBase
{
    protected JsonProblemStringReaderTestBase()
    {
        ProblemString = """
                        {
                            "title": "SomeProblem",
                            "type": "UnitTestProblem",
                            "detail": "This Unit Test was unexpectedly green",
                            "status": 42
                        }
                        """;
    }
}