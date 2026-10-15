using Newtonsoft.Json;
using RESTyard.Client.Extensions.NewtonsoftJson;

namespace Extensions.Test.SerializerTests;

public class NewtonsoftJsonObjectSerializerTest : JsonObjectSerializerTestBase
{
    public NewtonsoftJsonObjectSerializerTest()
    {
        Serializer = new NewtonsoftJsonObjectParameterSerializer(Formatting.Indented);
    }
}