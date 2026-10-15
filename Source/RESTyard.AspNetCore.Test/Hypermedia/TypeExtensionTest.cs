using System;
using RESTyard.AspNetCore.Util;

namespace RESTyard.AspNetCore.Test.Hypermedia
{
    public class TypeExtensionTest
    {
        [Fact]
        public void GetGenericTypeName()
        {
            var typeName = typeof(Nullable<int>).BeautifulName();
            Assert.AreEqual("Nullable<Int32>", typeName);
        }

        [Fact]
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