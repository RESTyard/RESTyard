using AwesomeAssertions;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Util;

namespace RESTyard.Client.Test.Util
{
    public class DistinctOrderedStringCollectionComparerTests
    {
        private DistinctOrderedStringCollectionComparer Comparer;

        public DistinctOrderedStringCollectionComparerTests()
        {
            this.Comparer = new DistinctOrderedStringCollectionComparer();
        }

        [Fact]
        public void HashCodeTest()
        {
            IDistinctOrderedCollection<string> animals =
                new DistinctOrderedStringCollection("elephant", "giraffe", "ant");
            var hashCode = this.Comparer.GetHashCode(animals);
            var hashCodeAgain = this.Comparer.GetHashCode(animals);
            hashCodeAgain.Should().Be(hashCode);

            IDistinctOrderedCollection<string> animalsAgain =
                new DistinctOrderedStringCollection("giraffe", "elephant", "ant");
            this.Comparer.GetHashCode(animalsAgain).Should().Be(hashCode);
        }

        [Fact]
        public void EqualsTest()
        {
            IDistinctOrderedCollection<string> animals = new DistinctOrderedStringCollection("lion", "elephant", "cat");
            this.Comparer.Equals(animals, animals).Should().BeTrue();

            this.Comparer.Equals(animals, null).Should().BeFalse();
            this.Comparer.Equals(null, animals).Should().BeFalse();

            IDistinctOrderedCollection<string> animalsAgain =
                new DistinctOrderedStringCollection("cat", "cat", "elephant", "lion");
            this.Comparer.Equals(animals, animalsAgain).Should().BeTrue();
            this.Comparer.Equals(animalsAgain, animals).Should().BeTrue();

            IDistinctOrderedCollection<string> numbers = new DistinctOrderedStringCollection("1", "2", "3");
            this.Comparer.Equals(animals, numbers).Should().BeFalse();
            this.Comparer.Equals(numbers, animals).Should().BeFalse();
        }
    }
}