using Bluehands.Hypermedia.Client.Extensions.NewtonsoftJson;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

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