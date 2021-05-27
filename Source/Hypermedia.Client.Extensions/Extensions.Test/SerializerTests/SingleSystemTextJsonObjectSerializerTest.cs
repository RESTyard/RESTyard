using System.Text.Json;
using Bluehands.Hypermedia.Client.Extensions.SystemTextJsonStringParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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