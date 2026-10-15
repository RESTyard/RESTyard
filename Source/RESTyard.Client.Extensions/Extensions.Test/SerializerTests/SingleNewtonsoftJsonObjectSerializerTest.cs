using Newtonsoft.Json;
using RESTyard.Client.Extensions.NewtonsoftJson;

namespace Extensions.Test.SerializerTests;

public class SingleNewtonsoftJsonObjectSerializerTest : SingleJsonObjectSerializerTestBase
{
    public SingleNewtonsoftJsonObjectSerializerTest()
    {
        Serializer = new SingleNewtonsoftJsonObjectParameterSerializer(Formatting.Indented);
    }
}