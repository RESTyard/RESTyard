using System.Text.Json;
using Bluehands.Hypermedia.Client.Extensions.SystemTextJsonStringParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Extensions.Test.SerializerTests
{
    [TestClass]
    public class SystemTextJsonObjectSerializerTest : JsonObjectSerializerTestBase
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            Serializer = new SystemTextJsonObjectParameterSerializer(new JsonSerializerOptions() { WriteIndented = true });
        }
    }
}