using RESTyard.Client.Extensions.SystemTextJson;

namespace Extensions.Test.StringParserTests;

public class SystemTextJsonParserTest : JsonParserTestBase
{
    public SystemTextJsonParserTest()
    {
        Parser = new SystemTextJsonStringParser();
    }
}