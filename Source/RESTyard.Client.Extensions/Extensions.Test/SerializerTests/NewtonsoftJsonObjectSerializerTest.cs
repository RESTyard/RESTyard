using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RESTyard.Client.Extensions.NewtonsoftJson;

namespace Extensions.Test.SerializerTests
{
    [TestClass]
    public class NewtonsoftJsonObjectSerializerTest : JsonObjectSerializerTestBase
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            Serializer = new NewtonsoftJsonObjectParameterSerializer(Formatting.Indented);
        }
    }
}