using Bluehands.Hypermedia.Client.ParameterSerializer;
using Extensions.Test.Hco;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Extensions.Test.SerializerTests
{
    public class SerializerTestBase
    {
        protected string ExpectedResult { get; set; }

        protected IParameterSerializer Serializer { get; set; }

        [TestMethod]
        public void SerializationTest()
        {
            var hco = new CustomerHco()
            {
                Address = "TestWeg",
                Age = 4,
                FullName = "TestCustomer",
                IsFavorite = true,
            };

            var serialized = Serializer.SerializeParameterObject("customer", hco);
            serialized.Should().Be(ExpectedResult);
        }
    }
}