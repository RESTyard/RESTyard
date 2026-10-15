using AwesomeAssertions;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Util;

namespace RESTyard.Client.Test.Util
{
    public class DistinctOrderedStringCollectionTests
    {
        [Fact]
        public void SingleElementTest()
        {
            IDistinctOrderedCollection<string> elephant = new DistinctOrderedStringCollection("elephant");
            elephant.Should().ContainSingle().Which.Should().Be("elephant");

            IDistinctOrderedCollection<string> giraffe = new DistinctOrderedStringCollection("giraffe");
            giraffe.Should().ContainSingle().Which.Should().Be("giraffe");
        }

        [Fact]
        public void MultipleElementTest_NoCollisions()
        {
            IDistinctOrderedCollection<string> animals = new DistinctOrderedStringCollection("elephant", "giraffe", "lion");
            animals.Should().HaveCount(3);
            animals.Should().Equal("elephant", "giraffe", "lion");

            IDistinctOrderedCollection<string> numbers =
                new DistinctOrderedStringCollection("2", "4", "3", "7", "5");
            numbers.Should().HaveCount(5);
            numbers.Should().Equal("2", "3", "4", "5", "7");
        }

        [Fact]
        public void MultipleElementTest_WithCollisions()
        {
            IDistinctOrderedCollection<string> animals =
                new DistinctOrderedStringCollection("giraffe", "elephant", "elephant");
            animals.Should().HaveCount(2);
            animals.Should().Equal("elephant", "giraffe");

            IDistinctOrderedCollection<string>
                numbers = new DistinctOrderedStringCollection("4", "17", "5", "5", "8", "4");
            numbers.Should().HaveCount(4);
            numbers.Should().Equal("17", "4", "5", "8");

        }
    }
}