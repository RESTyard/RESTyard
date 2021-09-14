using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Util;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bluehands.Hypermedia.Client.Test.Util
{
    [TestClass]
    public class DistinctOrderedStringCollectionComparerTests
    {
        private DistinctOrderedStringCollectionComparer Comparer;

        [TestInitialize]
        public void Setup()
        {
            this.Comparer = new DistinctOrderedStringCollectionComparer();
        }

        [TestMethod]
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

        [TestMethod]
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