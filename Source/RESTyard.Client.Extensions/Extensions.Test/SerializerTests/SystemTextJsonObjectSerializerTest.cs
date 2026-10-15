using System.Text.Json;
using RESTyard.Client.Extensions.SystemTextJson;

namespace Extensions.Test.SerializerTests;

public class SystemTextJsonObjectSerializerTest : JsonObjectSerializerTestBase
{
    public SystemTextJsonObjectSerializerTest()
    {
        Serializer = new SystemTextJsonObjectParameterSerializer(new JsonSerializerOptions() { WriteIndented = true });
    }
}