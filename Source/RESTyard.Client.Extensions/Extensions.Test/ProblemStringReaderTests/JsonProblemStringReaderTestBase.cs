namespace Extensions.Test.ProblemStringReaderTests
{
    public class JsonProblemStringReaderTestBase : ProblemStringReaderTestBase
    {
        public virtual void Initializer()
        {
            ProblemString = @"{
    ""title"": ""SomeProblem"",
    ""type"": ""UnitTestProblem"",
    ""detail"": ""This Unit Test was unexpectedly green"",
    ""status"": 42
}";
        }
    }
}