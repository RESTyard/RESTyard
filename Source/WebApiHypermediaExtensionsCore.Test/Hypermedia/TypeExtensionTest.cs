using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiHypermediaExtensionsCore.Util;

namespace WebApiHypermediaExtensionsCore.Test.Hypermedia
{
    [TestClass]
    public class TypeExtensionTest
    {
        [TestMethod]
        [Ignore]
        public void GetGenericTypeName()
        {
            var typeName = typeof(Nullable<int>).BeautifulName();
            Assert.AreEqual("Nullable<int>", typeName);
        }

        [TestMethod]
        [Ignore]
        public void GetNestedTypeName()
        {
            var typeName = typeof(Outer.Inner).BeautifulName();
            Assert.AreEqual("Outer.Inner", typeName);
        }

        class Outer
        {
            public class Inner
            {
            }
        }
    }
}