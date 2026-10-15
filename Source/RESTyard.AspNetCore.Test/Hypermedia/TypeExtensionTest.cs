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
            typeName.Should().Be("Nullable<Int32>");
        }

        [Fact]
        public void GetNestedTypeName()
        {
            var typeName = typeof(Outer.Inner).BeautifulName();
            typeName.Should().Be($"{nameof(TypeExtensionTest)}.{nameof(Outer)}.{nameof(Outer.Inner)}");
        }

        class Outer
        {
            public class Inner
            {
            }
        }
    }
}