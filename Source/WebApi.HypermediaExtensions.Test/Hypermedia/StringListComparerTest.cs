using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.AspNetCore.Util;

namespace RESTyard.WebApi.Extensions.Test.Hypermedia
{
    [TestClass]
    public class StringListComparerTest
    {
        private StringReadOnlyCollectionComparer stringReadOnlyListComparer;

        [TestInitialize]
        public void TestInit()
        {
            stringReadOnlyListComparer = new StringReadOnlyCollectionComparer();
        }

        [TestMethod]
        public void Compare_Null_ReturnsTrue()
        {
            Assert.IsTrue(stringReadOnlyListComparer.Equals(null, null));
        }

        [TestMethod]
        public void Compare_SameList_ReturnsTrue()
        {
            var listA = new List<string> { "" };

            Assert.IsTrue(stringReadOnlyListComparer.Equals(listA, listA));
        }

        [TestMethod]
        public void Compare_EmptyNull_ReturnsFalse()
        {
            var listA = new List<string> {""};
            Assert.IsFalse(stringReadOnlyListComparer.Equals(listA, null));
        }

        [TestMethod]
        public void Compare_BothEmpty_ReturnsTrue()
        {
            var listA = new List<string> { "" };
            var listB = new List<string> { "" };

            Assert.IsTrue(stringReadOnlyListComparer.Equals(listA, listB));
        }

        [TestMethod]
        public void Compare_Equal_ReturnsTrue()
        {
            var listA = new List<string> { "A" };
            var listB = new List<string> { "A" };

            Assert.IsTrue(stringReadOnlyListComparer.Equals(listA, listB));
        }

        [TestMethod]
        public void Compare_OneEmptyOneSet_ReturnsFalse()
        {
            var listA = new List<string> { "A" };
            var listB = new List<string> { "" };

            Assert.IsFalse(stringReadOnlyListComparer.Equals(listA, listB));
        }

        [TestMethod]
        public void Compare_OneListLarger_ReturnsFalse()
        {
            var listA = new List<string> { "A", "B" };
            var listB = new List<string> { "A" };

            Assert.IsFalse(stringReadOnlyListComparer.Equals(listA, listB));
        }

        [TestMethod]
        public void Compare_SameContentOrderDifferent_ReturnsTrue()
        {
            var listA = new List<string> { "B", "A" };
            var listB = new List<string> { "A", "B"};

            Assert.IsTrue(stringReadOnlyListComparer.Equals(listA, listB));
        }

        [TestMethod]
        public void Compare_SameContentLongOrderDifferent_ReturnsTrue()
        {
            var listA = new List<string> { "Abc", "Def" };
            var listB = new List<string> { "Def", "Abc" };

            Assert.IsTrue(stringReadOnlyListComparer.Equals(listA, listB));
        }


    }
}
