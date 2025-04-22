using FluentAssertions;
using RESTyard.HtoSourceGenerators.Test.Htos;

namespace RESTyard.HtoSourceGenerators.Test;

public class TestGeneratedClasses
{
    [Fact]
    public void CreateInstance()
    {
        var f = PropertyHtoSiren.FromHto(new PropertyHto());
        f.Should().NotBeNull();
        f.properties.Should().NotBeNull();
    }
}