namespace Extensions.Test.ProblemStringReaderTests
{
    public class JsonProblemStringReaderTestBase : ProblemStringReaderTestBase
    {
        public virtual void Initializer()
        {
            ProblemString = @"{
    ""Title"": ""SomeProblem"",
    ""Type"": ""UnitTestProblem"",
    ""Detail"": ""This Unit Test was unexpectedly green"",
    ""Status"": 42
}";
        }
    }
}