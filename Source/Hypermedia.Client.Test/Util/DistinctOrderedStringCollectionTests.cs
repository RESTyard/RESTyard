using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Util;

namespace RESTyard.Client.Test.Util
{
    [TestClass]
    public class DistinctOrderedStringCollectionTests
    {
        [TestMethod]
        public void SingleElementTest()
        {
            IDistinctOrderedCollection<string> elephant = new DistinctOrderedStringCollection("elephant");
            elephant.Count.Should().Be(1);
            elephant.Should().BeEquivalentTo("elephant");

            IDistinctOrderedCollection<string> giraffe = new DistinctOrderedStringCollection("giraffe");
            giraffe.Count.Should().Be(1);
            giraffe.Should().BeEquivalentTo("giraffe");
        }

        [TestMethod]
        public void MultipleElementTest_NoCollisions()
        {
            IDistinctOrderedCollection<string> animals = new DistinctOrderedStringCollection("elephant", "giraffe", "lion");
            animals.Count.Should().Be(3);
            animals.Should().BeEquivalentTo("elephant", "giraffe", "lion");

            IDistinctOrderedCollection<string> numbers =
                new DistinctOrderedStringCollection("2", "4", "3", "7", "5");
            numbers.Count.Should().Be(5);
            numbers.Should().BeEquivalentTo("2", "3", "4", "5", "7");
        }

        [TestMethod]
        public void MultipleElementTest_WithCollisions()
        {
            IDistinctOrderedCollection<string> animals =
                new DistinctOrderedStringCollection("giraffe", "elephant", "elephant");
            animals.Count.Should().Be(2);
            animals.Should().BeEquivalentTo("elephant", "giraffe");

            IDistinctOrderedCollection<string>
                numbers = new DistinctOrderedStringCollection("4", "17", "5", "5", "8", "4");
            numbers.Count.Should().Be(4);
            numbers.Should().BeEquivalentTo("4", "5", "8", "17");

        }
    }
}