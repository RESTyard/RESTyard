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
            stringReadOnlyListComparer.Equals(null, null).Should().BeTrue();
        }

        [Fact]
        public void Compare_SameList_ReturnsTrue()
        {
            var listA = new List<string> { "" };

            stringReadOnlyListComparer.Equals(listA, listA).Should().BeTrue();
        }

        [Fact]
        public void Compare_EmptyNull_ReturnsFalse()
        {
            var listA = new List<string> {""};
            stringReadOnlyListComparer.Equals(listA, null).Should().BeFalse();
        }

        [Fact]
        public void Compare_BothEmpty_ReturnsTrue()
        {
            var listA = new List<string> { "" };
            var listB = new List<string> { "" };

            stringReadOnlyListComparer.Equals(listA, listB).Should().BeTrue();
        }

        [Fact]
        public void Compare_Equal_ReturnsTrue()
        {
            var listA = new List<string> { "A" };
            var listB = new List<string> { "A" };

            stringReadOnlyListComparer.Equals(listA, listB).Should().BeTrue();
        }

        [Fact]
        public void Compare_OneEmptyOneSet_ReturnsFalse()
        {
            var listA = new List<string> { "A" };
            var listB = new List<string> { "" };

            stringReadOnlyListComparer.Equals(listA, listB).Should().BeFalse();
        }

        [Fact]
        public void Compare_OneListLarger_ReturnsFalse()
        {
            var listA = new List<string> { "A", "B" };
            var listB = new List<string> { "A" };

            stringReadOnlyListComparer.Equals(listA, listB).Should().BeFalse();
        }

        [Fact]
        public void Compare_SameContentOrderDifferent_ReturnsTrue()
        {
            var listA = new List<string> { "B", "A" };
            var listB = new List<string> { "A", "B"};

            stringReadOnlyListComparer.Equals(listA, listB).Should().BeTrue();
        }

        [Fact]
        public void Compare_SameContentLongOrderDifferent_ReturnsTrue()
        {
            var listA = new List<string> { "Abc", "Def" };
            var listB = new List<string> { "Def", "Abc" };

            stringReadOnlyListComparer.Equals(listA, listB).Should().BeTrue();
        }
    }
}
