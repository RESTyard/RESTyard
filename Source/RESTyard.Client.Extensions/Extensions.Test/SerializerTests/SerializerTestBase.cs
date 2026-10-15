using Extensions.Test.Hco;
using AwesomeAssertions;
using RESTyard.Client.ParameterSerializer;

namespace Extensions.Test.SerializerTests;

public abstract class SerializerTestBase
{
    protected string ExpectedResult { get; set; }

    protected IParameterSerializer Serializer { get; set; }

    [Fact]
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