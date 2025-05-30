﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.Test.WebApi.Formatter.Properties;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using RESTyard.Relations;

namespace RESTyard.AspNetCore.Test.WebApi.Formatter
{
    [TestClass]
    public class SirenBuilderEntitiesTest : SirenBuilderTestBase
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            ClassInitBase();
        }

        [TestInitialize]
        public void TestInit()
        {
            TestInitBase();
        }

        [TestMethod]
        public void RepresentationEntitiesTest()
        {
            var routeName = nameof(EmptyHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(EmptyHypermediaObject), routeName, HttpMethod.GET);

            var routeNameEmbedded = nameof(EmbeddedSubEntity) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(EmbeddedSubEntity), routeNameEmbedded, HttpMethod.GET);

            var ho = new EmptyHypermediaObject();
            var relation1 = "Embedded";
            var embeddedHo1 = new EmbeddedSubEntity();
            ho.Embedded.Add(EmbeddedEntity.Embed(embeddedHo1));

            var relationsList2 = new List<string> { "RelationA", "RelationB" };
            var embeddedHo2 = new EmbeddedSubEntity {ABool = true, AInt = 3};
            ho.Multiple.Add(EmbeddedEntity.Embed(embeddedHo2));

            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(EmptyHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyActions(siren);
            AssertHasNoLinks(siren);

            Assert.IsTrue(siren["entities"].Type == JTokenType.Array);
            var entitiesArray = (JArray)siren["entities"];
            Assert.AreEqual(entitiesArray.Count, 2);

            var embeddedEntityObject = (JObject)siren["entities"][0];
            AssertClassName(embeddedEntityObject, nameof(EmbeddedSubEntity));
            AssertRelations(embeddedEntityObject, new List<string> { relation1 });
            AssertHasOnlySelfLink(embeddedEntityObject, routeNameEmbedded);
            AssertEmbeddedEntity(embeddedEntityObject, embeddedHo1);

            embeddedEntityObject = (JObject)siren["entities"][1];
            AssertClassName(embeddedEntityObject, nameof(EmbeddedSubEntity));
            AssertRelations(embeddedEntityObject, relationsList2);
            AssertHasOnlySelfLink(embeddedEntityObject, routeNameEmbedded);
            AssertEmbeddedEntity(embeddedEntityObject, embeddedHo2);
        }

        [TestMethod]
        public void LinkEntitiesTest()
        {
            var routeName = nameof(EmptyHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(EmptyHypermediaObject), routeName, HttpMethod.GET);

            var routeNameEmbedded = nameof(EmbeddedSubEntity) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(EmbeddedSubEntity), routeNameEmbedded, HttpMethod.GET);
            RouteRegister.AddRouteKeyProducer(typeof(EmbeddedSubEntity), new EmbeddedEntityRouteKeyProducer());

            var ho = new EmptyHypermediaObject();

            var relation1 = "Embedded";
            ho.Embedded.Add(new EmbeddedEntity<EmbeddedSubEntity>(new HypermediaObjectKeyReference(typeof(EmbeddedSubEntity), 6)));

            var relationsList2 = new List<string> { "RelationA", "RelationB" };
            var query = new EmbeddedQueryObject {AInt = 2};
            ho.Multiple.Add(new EmbeddedEntity<EmbeddedSubEntity>(new HypermediaObjectQueryReference(typeof(EmbeddedSubEntity), query, 3)));

            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(EmptyHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyActions(siren);
            AssertHasNoLinks(siren);

            Assert.IsTrue(siren["entities"].Type == JTokenType.Array);
            var entitiesArray = (JArray)siren["entities"];
            Assert.AreEqual(entitiesArray.Count, 2);

            var embeddedEntityObject = (JObject)siren["entities"][0];
            AssertRelations(embeddedEntityObject, new List<string> { relation1 });
            AssertRoute(((JValue)embeddedEntityObject["href"]).Value<string>(), routeNameEmbedded, "{ key = 6 }");

            embeddedEntityObject = (JObject)siren["entities"][1];
            AssertRelations(embeddedEntityObject, relationsList2);
            AssertRoute(((JValue)embeddedEntityObject["href"]).Value<string>(), routeNameEmbedded, "{ key = 3 }", QueryStringBuilder.CreateQueryString(query));
        }

        private static void AssertEmbeddedEntity(JObject embeddedEntityObject, EmbeddedSubEntity embeddedSubHo)
        {
            var embeddedEntityProperties = (JObject)embeddedEntityObject["properties"];
            Assert.AreEqual(embeddedEntityProperties.Count, 2);
            Assert.AreEqual(embeddedEntityObject["properties"]["ABool"].ToString(), embeddedSubHo.ABool.ToString());
            Assert.AreEqual(embeddedEntityObject["properties"]["AInt"].ToString(), embeddedSubHo.AInt.ToString());
        }

        public static void AssertRelations(JObject obj, List<string> relations)
        {
            Assert.IsTrue(obj["rel"].Type == JTokenType.Array);
            var relArray = (JArray)obj["rel"];
            Assert.AreEqual(relArray.Count, relations.Count);

            foreach (var relation in relations)
            {
                var hasDesiredRelation = relArray.FirstOrDefault(i => i.Value<string>().Equals(relation)) != null;
                Assert.IsTrue(hasDesiredRelation);
            }
        }

        [HypermediaObject(Classes = [nameof(EmbeddedSubEntity)])]
        public class EmbeddedSubEntity : IHypermediaObject
        {
            public bool ABool { get; set; }
            public int AInt { get; set; }

            [Relations([DefaultHypermediaRelations.Self])]
            public ILink<EmbeddedSubEntity> Self => Link.To(this);
        }

        public class EmbeddedQueryObject : IHypermediaQuery
        {
            public int AInt { get; set; }
        }

        public class EmbeddedEntityRouteKeyProducer : IKeyProducer
        {
            public object CreateFromHypermediaObject(IHypermediaObject hypermediaObject)
            {
                throw new System.NotImplementedException();
            }

            public object CreateFromKeyObject(object keyObject)
            {
                return new {key = keyObject};
            }
        }
    }


}
