using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.Client.Extensions.SystemTextJson;

namespace Extensions.Test.SerializerTests
{
    [TestClass]
    public class SingleSystemTextJsonObjectSerializerTest : SingleJsonObjectSerializerTestBase
    {
        [TestInitialize]
        public override void Initializer()
        {
            base.Initializer();
            Serializer = new SingleSystemTextJsonObjectParameterSerializer(new JsonWriterOptions() { Indented = true, });
        }
    }
}