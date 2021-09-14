using Bluehands.Hypermedia.Client.Extensions.NewtonsoftJson;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

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