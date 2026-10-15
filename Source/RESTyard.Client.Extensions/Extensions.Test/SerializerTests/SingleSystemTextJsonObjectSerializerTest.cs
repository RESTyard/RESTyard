using System.Text.Json;
using RESTyard.Client.Extensions.SystemTextJson;

namespace Extensions.Test.SerializerTests;

public class SingleSystemTextJsonObjectSerializerTest : SingleJsonObjectSerializerTestBase
{
    public SingleSystemTextJsonObjectSerializerTest()
    {
        Serializer = new SingleSystemTextJsonObjectParameterSerializer(new JsonWriterOptions() { Indented = true, });
    }
}