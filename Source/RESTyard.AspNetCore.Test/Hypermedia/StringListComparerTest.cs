using System;
using System.Collections.Generic;

using RESTyard.AspNetCore.Util;

namespace RESTyard.AspNetCore.Test.Hypermedia
{
    public class StringListComparerTest
    {
        private StringReadOnlyCollectionComparer stringReadOnlyListComparer;

        public StringListComparerTest()
        {
            stringReadOnlyListComparer = new StringReadOnlyCollectionComparer();
        }

        [Fact]
        public void Compare_Null_ReturnsTrue()
        {
            Assert.IsTrue(stringReadOnlyListComparer.Equals(null, null));
        }

        [Fact]
        public void Compare_SameList_ReturnsTrue()
        {
            var listA = new List<string> { "" };

            Assert.IsTrue(stringReadOnlyListComparer.Equals(listA, listA));
        }

        [Fact]
        public void Compare_EmptyNull_ReturnsFalse()
        {
            var listA = new List<string> {""};
            Assert.IsFalse(stringReadOnlyListComparer.Equals(listA, null));
        }

        [Fact]
        public void Compare_BothEmpty_ReturnsTrue()
        {
            var listA = new List<string> { "" };
            var listB = new List<string> { "" };

            Assert.IsTrue(stringReadOnlyListComparer.Equals(listA, listB));
        }

        [Fact]
        public void Compare_Equal_ReturnsTrue()
        {
            var listA = new List<string> { "A" };
            var listB = new List<string> { "A" };

            Assert.IsTrue(stringReadOnlyListComparer.Equals(listA, listB));
        }

        [Fact]
        public void Compare_OneEmptyOneSet_ReturnsFalse()
        {
            var listA = new List<string> { "A" };
            var listB = new List<string> { "" };

            Assert.IsFalse(stringReadOnlyListComparer.Equals(listA, listB));
        }

        [Fact]
        public void Compare_OneListLarger_ReturnsFalse()
        {
            var listA = new List<string> { "A", "B" };
            var listB = new List<string> { "A" };

            Assert.IsFalse(stringReadOnlyListComparer.Equals(listA, listB));
        }

        [Fact]
        public void Compare_SameContentOrderDifferent_ReturnsTrue()
        {
            var listA = new List<string> { "B", "A" };
            var listB = new List<string> { "A", "B"};

            Assert.IsTrue(stringReadOnlyListComparer.Equals(listA, listB));
        }

        [Fact]
        public void Compare_SameContentLongOrderDifferent_ReturnsTrue()
        {
            var listA = new List<string> { "Abc", "Def" };
            var listB = new List<string> { "Def", "Abc" };

            Assert.IsTrue(stringReadOnlyListComparer.Equals(listA, listB));
        }
    }
}
