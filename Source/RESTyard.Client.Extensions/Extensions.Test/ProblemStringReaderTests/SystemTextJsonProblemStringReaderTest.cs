using RESTyard.Client.Extensions.SystemTextJson;

namespace Extensions.Test.ProblemStringReaderTests;

public class SystemTextJsonProblemStringReaderTest : JsonProblemStringReaderTestBase
{
    public SystemTextJsonProblemStringReaderTest()
    {
        ProblemReader = new SystemTextJsonProblemStringReader();
    }
}