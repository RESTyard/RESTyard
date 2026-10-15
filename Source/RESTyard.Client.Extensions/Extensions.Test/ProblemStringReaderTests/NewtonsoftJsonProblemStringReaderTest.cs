
using RESTyard.Client.Extensions.NewtonsoftJson;

namespace Extensions.Test.ProblemStringReaderTests;

public class NewtonsoftJsonProblemStringReaderTest : JsonProblemStringReaderTestBase
{
    public NewtonsoftJsonProblemStringReaderTest()
    {
        ProblemReader = new NewtonsoftJsonProblemStringReader();
    }
}