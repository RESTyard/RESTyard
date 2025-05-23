using System;
using System.Collections.Generic;
using System.Linq;
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

            public record KeyRecord(string Key) : HypermediaObjectKeyBase<MyHypermediaObject>
            {
                protected override IEnumerable<KeyValuePair<string, object?>> EnumerateKeysForLinkGeneration()
                {
                    yield return new KeyValuePair<string, object?>("key", this.Key);
                }
            }
        }

        [TestMethod]
        public void GetKeyForObject()
        {
            dynamic key = routeKeyProducer.CreateFromHypermediaObject(new MyHypermediaObject("valueOfKey1"));

            ((string)key.id1).Should().Be("valueOfKey1");
        }

        [TestMethod]
        public void GetKeyFromKeyObject()
        {
            var key = new MyHypermediaObject.KeyRecord("valueOfKey");
            var result = routeKeyProducer.CreateFromKeyObject(key);

            var kvp = result.Should().BeAssignableTo<IEnumerable<KeyValuePair<string, object?>>>().Which.Should().ContainSingle().Which;
            kvp.Key.Should().Be("key");
            kvp.Value.Should().BeOfType<string>().Which.Should().Be("valueOfKey");
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

            public record KeyRecord(string Key1, int Key2) : HypermediaObjectKeyBase<MyHypermediaObject>
            {
                protected override IEnumerable<KeyValuePair<string, object?>> EnumerateKeysForLinkGeneration()
                {
                    yield return new KeyValuePair<string, object?>("key1", this.Key1);
                    yield return new KeyValuePair<string, object?>("key2", this.Key2);
                }
            }
        }

        [TestMethod]
        public void GetKeyForObject()
        {
            dynamic key = routeKeyProducer.CreateFromHypermediaObject(new MyHypermediaObject("valueOfKey1", 2));

            ((string)key.id1).Should().Be("valueOfKey1");
            ((int) key.id2).Should().Be(2);
        }

        [TestMethod]
        public void GetKeyFromKeyObject()
        {
            var key = new MyHypermediaObject.KeyRecord("valueOfKey1", 2);
            var result = routeKeyProducer.CreateFromKeyObject(key);

            var values = result.Should().BeAssignableTo<IEnumerable<KeyValuePair<string, object?>>>().Which.ToList();
            values.Should().HaveCount(2);
            var key1 = values[0];
            key1.Key.Should().Be("key1");
            key1.Value.Should().BeOfType<string>().Which.Should().Be("valueOfKey1");

            var key2 = values[1];
            key2.Key.Should().Be("key2");
            key2.Value.Should().BeOfType<int>().Which.Should().Be(2);
            
        }
    }
}
