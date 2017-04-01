using System.Collections.Generic;
using Hypermedia.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebApiHypermediaExtensionsCore.Test.Hypermedia
{
    [TestClass]
    public class StringListComparerTest
    {
        private StringCollectionComparer stringListComparer;

        [TestInitialize]
        public void TestInit()
        {
            stringListComparer = new StringCollectionComparer();
        }

        [TestMethod]
        public void Compare_Null_ReturnsTrue()
        {
            Assert.IsTrue(stringListComparer.Equals(null, null));
        }

        [TestMethod]
        public void Compare_SameList_ReturnsTrue()
        {
            var listA = new List<string> { "" };

            Assert.IsTrue(stringListComparer.Equals(listA, listA));
        }

        [TestMethod]
        public void Compare_EmptyNull_ReturnsFalse()
        {
            var listA = new List<string> {""};
            Assert.IsFalse(stringListComparer.Equals(listA, null));
        }

        [TestMethod]
        public void Compare_BothEmpty_ReturnsTrue()
        {
            var listA = new List<string> { "" };
            var listB = new List<string> { "" };

            Assert.IsTrue(stringListComparer.Equals(listA, listB));
        }

        [TestMethod]
        public void Compare_Equal_ReturnsTrue()
        {
            var listA = new List<string> { "A" };
            var listB = new List<string> { "A" };

            Assert.IsTrue(stringListComparer.Equals(listA, listB));
        }

        [TestMethod]
        public void Compare_OneEmptyOneSet_ReturnsFalse()
        {
            var listA = new List<string> { "A" };
            var listB = new List<string> { "" };

            Assert.IsFalse(stringListComparer.Equals(listA, listB));
        }

        [TestMethod]
        public void Compare_OneListLarger_ReturnsFalse()
        {
            var listA = new List<string> { "A", "B" };
            var listB = new List<string> { "A" };

            Assert.IsFalse(stringListComparer.Equals(listA, listB));
        }

        [TestMethod]
        public void Compare_SameContentOrderDifferent_ReturnsTrue()
        {
            var listA = new List<string> { "B", "A" };
            var listB = new List<string> { "A", "B"};

            Assert.IsTrue(stringListComparer.Equals(listA, listB));
        }

        [TestMethod]
        public void Compare_SameContentLongOrderDifferent_ReturnsTrue()
        {
            var listA = new List<string> { "Abc", "Def" };
            var listB = new List<string> { "Def", "Abc" };

            Assert.IsTrue(stringListComparer.Equals(listA, listB));
        }


    }
}
