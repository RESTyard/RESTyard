using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.WebApi.Extensions.Util;

namespace RESTyard.WebApi.Extensions.Test.Hypermedia
{
    [TestClass]
    public class TypeExtensionTest
    {
        [TestMethod]
        public void GetGenericTypeName()
        {
            var typeName = typeof(Nullable<int>).BeautifulName();
            Assert.AreEqual("Nullable<Int32>", typeName);
        }

        [TestMethod]
        public void GetNestedTypeName()
        {
            var typeName = typeof(Outer.Inner).BeautifulName();
            Assert.AreEqual($"{nameof(TypeExtensionTest)}.{nameof(Outer)}.{nameof(Outer.Inner)}", typeName);
        }

        class Outer
        {
            public class Inner
            {
            }
        }
    }
}