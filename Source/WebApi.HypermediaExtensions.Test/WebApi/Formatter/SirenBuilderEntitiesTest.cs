using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Extensions;
using WebApi.HypermediaExtensions.Hypermedia.Links;
using WebApi.HypermediaExtensions.Query;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.Test.WebApi.Formatter
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

            var routeNameEmbedded = typeof(EmbeddedSubEntity).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(EmbeddedSubEntity), routeNameEmbedded);

            var ho = new EmptyHypermediaObject();
            var relation1 = "Embedded";
            var embeddedHo1 = new EmbeddedSubEntity();
            ho.Entities.Add(relation1, new HypermediaObjectReference(embeddedHo1));

            var relationsList2 = new List<string> { "RelationA", "RelationB" };
            var embeddedHo2 = new EmbeddedSubEntity {ABool = true, AInt = 3};
            ho.Entities.Add(new RelatedEntity(relationsList2, new HypermediaObjectReference(embeddedHo2)));

            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(EmptyHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            Assert.IsTrue(siren["entities"].Type == JTokenType.Array);
            var entitiesArray = (JArray)siren["entities"];
            Assert.AreEqual(entitiesArray.Count, ho.Entities.Count);

            var embeddedEntityObject = (JObject)siren["entities"][0];
            AssertDefaultClassName(embeddedEntityObject, typeof(EmbeddedSubEntity));
            AssertRelations(embeddedEntityObject, new List<string> { relation1 });
            AssertHasOnlySelfLink(embeddedEntityObject, routeNameEmbedded);
            AssertEmbeddedEntity(embeddedEntityObject, embeddedHo1);

            embeddedEntityObject = (JObject)siren["entities"][1];
            AssertDefaultClassName(embeddedEntityObject, typeof(EmbeddedSubEntity));
            AssertRelations(embeddedEntityObject, relationsList2);
            AssertHasOnlySelfLink(embeddedEntityObject, routeNameEmbedded);
            AssertEmbeddedEntity(embeddedEntityObject, embeddedHo2);
        }

        [TestMethod]
        public void LinkEntitiesTest()
        {
            var routeName = typeof(EmptyHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(EmptyHypermediaObject), routeName);

            var routeNameEmbedded = typeof(EmbeddedSubEntity).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(EmbeddedSubEntity), routeNameEmbedded);
            RouteRegister.AddRouteKeyProducer(typeof(EmbeddedSubEntity), new EmbeddedEntityRouteKeyProducer());

            var ho = new EmptyHypermediaObject();

            var relation1 = "Embedded";
            ho.Entities.Add(relation1, new HypermediaObjectKeyReference(typeof(EmbeddedSubEntity), 6));

            var relationsList2 = new List<string> { "RelationA", "RelationB" };
            var query = new EmbeddedQueryObject {AInt = 2};
            ho.Entities.Add(new RelatedEntity(relationsList2, new HypermediaObjectQueryReference(typeof(EmbeddedSubEntity), query, 3)));

            var siren = SirenConverter.ConvertToJson(ho);

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

        public class EmbeddedSubEntity : HypermediaObject
        {
            public bool ABool { get; set; }
            public int AInt { get; set; }
        }

        public class EmbeddedQueryObject : IHypermediaQuery
        {
            public int AInt { get; set; }
        }

        public class EmbeddedEntityRouteKeyProducer : IKeyProducer
        {
            public object CreateFromHypermediaObject(HypermediaObject hypermediaObject)
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
