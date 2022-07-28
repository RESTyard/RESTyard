namespace Extensions.Test.ProblemStringReaderTests
{
    public class JsonProblemStringReaderTestBase : ProblemStringReaderTestBase
    {
        public virtual void Initializer()
        {
            ProblemString = @"{
    ""Title"": ""SomeProblem"",
    ""ProblemType"": ""UnitTestProblem"",
    ""Detail"": ""This Unit Test was unexpectedly green"",
    ""StatusCode"": 42
}";
        }
    }
}