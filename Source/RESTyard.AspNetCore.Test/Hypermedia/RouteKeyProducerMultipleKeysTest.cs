using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.Test.Hypermedia
{


    [TestClass]
    public class RouteKeyProducerSingleKeyTest
    {
        RouteKeyProducer routeKeyProducer;

        [TestInitialize]
        public void TestInit()
        {
            routeKeyProducer = RouteKeyProducer.Create(typeof(MyHypermediaObject), new[] { "id1" });
        }

        class MyHypermediaObject : IHypermediaObject
        {
            [Key]
            public string Key { get; }

            public MyHypermediaObject(string key1)
            {
                Key = key1;
            }
        }

        [TestMethod]
        public void GetKeyForObject()
        {
            dynamic key = routeKeyProducer.CreateFromHypermediaObject(new MyHypermediaObject("valueOfKey1"));

            ((string)key.id1).Should().Be("valueOfKey1");
        }
    }

    [TestClass]
    public class RouteKeyProducerMultipleKeysTest
    {
        RouteKeyProducer routeKeyProducer;

        [TestInitialize]
        public void TestInit()
        {
            routeKeyProducer = RouteKeyProducer.Create(typeof(MyHypermediaObject), new[] {"id2", "id1"});
        }

        class MyHypermediaObject : IHypermediaObject
        {
            [Key("id1")]
            public string Key1 { get; }

            [Key("id2")]
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
            dynamic key = routeKeyProducer.CreateFromHypermediaObject(new MyHypermediaObject("valueOfKey1", 2));

            ((string)key.id1).Should().Be("valueOfKey1");
            ((int) key.id2).Should().Be(2);
        }
    }
}
