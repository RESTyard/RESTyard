using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi.HypermediaExtensions.Hypermedia;
using FluentAssertions;
using WebApi.HypermediaExtensions.Util.Extensions;

namespace WebApi.Hypermedia.Serializer.Tests
{
    [TestClass]
    public class ExternalLinkTest
    {

        [TestInitialize]
        public void Initialize()
        {
     
        }

        [TestMethod]
        public void TestMethod1()
        {
            var prop = typeof(TestClass).GetProperty("Test").GetValueGetter<Uri>();
            var obj = new TestClass();
            prop.Invoke(obj).Should()
                .Be("https://weblogs.asp.net/marianor/using-expression-trees-to-get-property-getter-and-setters");

        }
    }

    class Base
    {
        
    }

    class TestClass : HypermediaObject
    {
        public Uri Test { get; set; } = new Uri("https://weblogs.asp.net/marianor/using-expression-trees-to-get-property-getter-and-setters");
    }
}
