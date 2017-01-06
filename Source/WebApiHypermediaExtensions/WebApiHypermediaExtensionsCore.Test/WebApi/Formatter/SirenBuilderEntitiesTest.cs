using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Extensions;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;
using WebApiHypermediaExtensionsCore.Query;

namespace WebApiHypermediaExtensionsCore.Test.WebApi.Formatter
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
            var routeName = typeof(EmptyHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(EmptyHypermediaObject), routeName);

            var routeNameEmbedded = typeof(EmbeddedEntity).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(EmbeddedEntity), routeNameEmbedded);

            var ho = new EmptyHypermediaObject();
            var relation1 = "Embedded";
            var embeddedHo1 = new EmbeddedEntity();
            ho.Entities.Add(relation1, new HypermediaObjectReference(embeddedHo1));

            var relationsList2 = new List<string> { "RelationA", "RelationB" };
            var embeddedHo2 = new EmbeddedEntity {ABool = true, AInt = 3};
            ho.Entities.Add(new Hypermedia.EmbeddedEntity(relationsList2, new HypermediaObjectReference(embeddedHo2)));

            var siren = SirenBuilder.CreateSirenJObject(ho, RouteResolver, QueryStringBuilder);

            AssertDefaultClassName(siren, typeof(EmptyHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            Assert.IsTrue(siren["entities"].Type == JTokenType.Array);
            var entitiesArray = (JArray)siren["entities"];
            Assert.AreEqual(entitiesArray.Count, ho.Entities.Count);

            var embeddedEntityObject = (JObject)siren["entities"][0];
            AssertDefaultClassName(embeddedEntityObject, typeof(EmbeddedEntity));
            AssertRelations(embeddedEntityObject, new List<string> { relation1 });
            AssertHasOnlySelfLink(embeddedEntityObject, routeNameEmbedded);
            AssertEmbeddedEntity(embeddedEntityObject, embeddedHo1);

            embeddedEntityObject = (JObject)siren["entities"][1];
            AssertDefaultClassName(embeddedEntityObject, typeof(EmbeddedEntity));
            AssertRelations(embeddedEntityObject, relationsList2);
            AssertHasOnlySelfLink(embeddedEntityObject, routeNameEmbedded);
            AssertEmbeddedEntity(embeddedEntityObject, embeddedHo2);
        }

        [TestMethod]
        public void LinkEntitiesTest()
        {
            var routeName = typeof(EmptyHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(EmptyHypermediaObject), routeName);

            var routeNameEmbedded = typeof(EmbeddedEntity).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(EmbeddedEntity), routeNameEmbedded);

            var ho = new EmptyHypermediaObject();

            var relation1 = "Embedded";
            ho.Entities.Add(relation1, new HypermediaObjectKeyReference(typeof(EmbeddedEntity), 6));

            var relationsList2 = new List<string> { "RelationA", "RelationB" };
            var query = new EmbeddedQueryObject {AInt = 2};
            ho.Entities.Add(new Hypermedia.EmbeddedEntity(relationsList2, new HypermediaObjectQueryReference(typeof(EmbeddedEntity), query, 3)));

            var siren = SirenBuilder.CreateSirenJObject(ho, RouteResolver, QueryStringBuilder);

            AssertDefaultClassName(siren, typeof(EmptyHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            Assert.IsTrue(siren["entities"].Type == JTokenType.Array);
            var entitiesArray = (JArray)siren["entities"];
            Assert.AreEqual(entitiesArray.Count, ho.Entities.Count);

            var embeddedEntityObject = (JObject)siren["entities"][0];
            AssertRelations(embeddedEntityObject, new List<string> { relation1 });
            AssertRoute(((JValue)embeddedEntityObject["href"]).Value<string>(), routeNameEmbedded, "{ key = 6 }");

            embeddedEntityObject = (JObject)siren["entities"][1];
            AssertRelations(embeddedEntityObject, relationsList2);
            AssertRoute(((JValue)embeddedEntityObject["href"]).Value<string>(), routeNameEmbedded, "{ key = 3 }", QueryStringBuilder.CreateQueryString(query));
        }

        private static void AssertEmbeddedEntity(JObject embeddedEntityObject, EmbeddedEntity embeddedHo)
        {
            var embeddedEntityProperties = (JObject)embeddedEntityObject["properties"];
            Assert.AreEqual(embeddedEntityProperties.Count, 2);
            Assert.AreEqual(embeddedEntityObject["properties"]["ABool"].ToString(), embeddedHo.ABool.ToString());
            Assert.AreEqual(embeddedEntityObject["properties"]["AInt"].ToString(), embeddedHo.AInt.ToString());
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

        public class EmbeddedEntity : HypermediaObject
        {
            public bool ABool { get; set; }
            public int AInt { get; set; }
        }

        public class EmbeddedQueryObject : IHypermediaQuery
        {
            public int AInt { get; set; }
        }
    }
}
