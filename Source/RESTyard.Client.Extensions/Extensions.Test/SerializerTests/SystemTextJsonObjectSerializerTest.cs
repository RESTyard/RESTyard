using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.Client.Extensions.SystemTextJson;

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