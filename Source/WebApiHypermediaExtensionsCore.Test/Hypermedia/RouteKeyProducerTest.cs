using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;
//using FluentAssertions;
using Microsoft.CodeAnalysis;

namespace WebApiHypermediaExtensionsCore.Test.Hypermedia
{
    [TestClass]
    public class RouteKeyProducerTest
    {
        RouteKeyProducer2 routeKeyProducer;

        [TestInitialize]
        public void TestInit()
        {
            routeKeyProducer = RouteKeyProducer2.Create(typeof(MyHypermediaObject), new[] {"id2, id1"});
        }

        class MyHypermediaObject : HypermediaObject
        {
            [RouteTemplateParameter("id1")]
            public string Key1 { get; }

            [RouteTemplateParameter("id2")]
            public int Key2 { get; }

            public MyHypermediaObject(string key1, int key2)
            {
                Key1 = key1;
                Key2 = key2;
            }
        }

        [TestMethod]
        public void GetKeyForObject()
        {
            //dynamic key = routeKeyProducer.CreateFromHypermediaObject(new MyHypermediaObject("valueOfKey1", 2));

            //key.id1.Should().Be("valueOfKey1");
            //key.id2.Should().Be(2);
        }
    }
}
