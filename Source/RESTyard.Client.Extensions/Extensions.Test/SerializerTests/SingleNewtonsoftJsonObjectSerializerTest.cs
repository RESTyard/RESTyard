using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RESTyard.Client.Extensions.NewtonsoftJson;

namespace Extensions.Test.SerializerTests
{
    [TestClass]
    public class SingleNewtonsoftJsonObjectSerializerTest : SingleJsonObjectSerializerTestBase
    {
        [TestInitialize]
        public override void Initializer()
        {
            base.Initializer();
            Serializer = new SingleNewtonsoftJsonObjectParameterSerializer(Formatting.Indented);
        }
    }
}