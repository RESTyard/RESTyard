using RESTyard.Client.Extensions.NewtonsoftJson;

namespace Extensions.Test.StringParserTests;

public class NewtonsoftJsonParserTest : JsonParserTestBase
{
    public NewtonsoftJsonParserTest()
    {
        Parser = new NewtonsoftJsonStringParser();
    }
}