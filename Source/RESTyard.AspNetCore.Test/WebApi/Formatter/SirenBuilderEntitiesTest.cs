using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Nodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            RouteRegister.AddHypermediaObjectRoute(typeof(EmptyHypermediaObject), routeName, HttpMethods.Get);

            var routeNameEmbedded = nameof(EmbeddedSubEntity) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(EmbeddedSubEntity), routeNameEmbedded, HttpMethods.Get);

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

            Assert.IsTrue(siren["entities"] is JsonArray);
            var entitiesArray = siren["entities"]!.AsArray();
            Assert.AreEqual(entitiesArray.Count, 2);

            var embeddedEntityObject = entitiesArray[0]!.AsObject();
            AssertClassName(embeddedEntityObject, nameof(EmbeddedSubEntity));
            AssertRelations(embeddedEntityObject, new List<string> { relation1 });
            AssertHasOnlySelfLink(embeddedEntityObject, routeNameEmbedded);
            AssertEmbeddedEntity(embeddedEntityObject, embeddedHo1);

            embeddedEntityObject = entitiesArray[1]!.AsObject();
            AssertClassName(embeddedEntityObject, nameof(EmbeddedSubEntity));
            AssertRelations(embeddedEntityObject, relationsList2);
            AssertHasOnlySelfLink(embeddedEntityObject, routeNameEmbedded);
            AssertEmbeddedEntity(embeddedEntityObject, embeddedHo2);
        }

        [TestMethod]
        public void LinkEntitiesTest()
        {
            var routeName = nameof(EmptyHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(EmptyHypermediaObject), routeName, HttpMethods.Get);

            var routeNameEmbedded = nameof(EmbeddedSubEntity) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(EmbeddedSubEntity), routeNameEmbedded, HttpMethods.Get);
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

            Assert.IsTrue(siren["entities"] is JsonArray);
            var entitiesArray = siren["entities"]!.AsArray();
            Assert.AreEqual(entitiesArray.Count, 2);

            var embeddedEntityObject = entitiesArray[0]!.AsObject();
            AssertRelations(embeddedEntityObject, new List<string> { relation1 });
            AssertRoute(embeddedEntityObject["href"]!.GetValue<string>(), routeNameEmbedded, "{ key = 6 }");

            embeddedEntityObject = entitiesArray[1]!.AsObject();
            AssertRelations(embeddedEntityObject, relationsList2);
            AssertRoute(embeddedEntityObject["href"]!.GetValue<string>(), routeNameEmbedded, "{ key = 3 }", QueryStringBuilder.CreateQueryString(query));
        }

        /// <summary>
        /// Embedded entities with duplicate relations must ALL be preserved.
        /// Unlike links (silently dedup), embedded entities iterate
        /// directly — both entities should appear in the output.
        /// </summary>
        [TestMethod]
        public void EmbeddedEntitiesDuplicateRelation_BothEntitiesPreserved()
        {
            RouteRegister.AddHypermediaObjectRoute(typeof(DuplicateRelEmbeddingHypermediaObject),
                nameof(DuplicateRelEmbeddingHypermediaObject) + "_Route", HttpMethods.Get);
            RouteRegister.AddHypermediaObjectRoute(typeof(EmbeddedSubEntity),
                nameof(EmbeddedSubEntity) + "_Route", HttpMethods.Get);
            RouteRegister.AddHypermediaObjectRoute(typeof(EmbeddedSubEntity2),
                nameof(EmbeddedSubEntity2) + "_Route", HttpMethods.Get);

            var ho = new DuplicateRelEmbeddingHypermediaObject
            {
                Embedded1 = EmbeddedEntity.Embed(new EmbeddedSubEntity { ABool = true, AInt = 42 }),
                Embedded2 = EmbeddedEntity.Embed(new EmbeddedSubEntity2 { AString = "hello" }),
            };

            var siren = SirenConverter.ConvertToJson(ho);

            Assert.IsTrue(siren["entities"] is JsonArray);
            var entitiesArray = siren["entities"]!.AsArray();
            Assert.AreEqual(2, entitiesArray.Count,
                "Both embedded entities with the same relation should be present");

            var entity1 = entitiesArray[0]!.AsObject();
            AssertClassName(entity1, nameof(EmbeddedSubEntity));
            AssertRelations(entity1, new List<string> { "Duplicate" });

            var entity2 = entitiesArray[1]!.AsObject();
            AssertClassName(entity2, nameof(EmbeddedSubEntity2));
            AssertRelations(entity2, new List<string> { "Duplicate" });
        }

        private static void AssertEmbeddedEntity(JsonObject embeddedEntityObject, EmbeddedSubEntity embeddedSubHo)
        {
            var embeddedEntityProperties = embeddedEntityObject["properties"]!.AsObject();
            Assert.AreEqual(embeddedEntityProperties.Count, 2);
            Assert.AreEqual(embeddedSubHo.ABool, embeddedEntityProperties["ABool"]!.GetValue<bool>());
            Assert.AreEqual(embeddedSubHo.AInt, embeddedEntityProperties["AInt"]!.GetValue<int>());
        }

        public static void AssertRelations(JsonObject obj, List<string> relations)
        {
            Assert.IsTrue(obj["rel"] is JsonArray);
            var relArray = obj["rel"]!.AsArray();
            Assert.AreEqual(relArray.Count, relations.Count);

            foreach (var relation in relations)
            {
                var hasDesiredRelation = relArray.FirstOrDefault(i => i!.GetValue<string>().Equals(relation)) != null;
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

        [HypermediaObject(Classes = [nameof(EmbeddedSubEntity2)])]
        public class EmbeddedSubEntity2 : IHypermediaObject
        {
            public string AString { get; set; } = string.Empty;

            [Relations([DefaultHypermediaRelations.Self])]
            public ILink<EmbeddedSubEntity2> Self => Link.To(this);
        }

        [HypermediaObject(Classes = [nameof(DuplicateRelEmbeddingHypermediaObject)])]
        public class DuplicateRelEmbeddingHypermediaObject : IHypermediaObject
        {
            [Relations(["Duplicate"])]
            public IEmbeddedEntity<EmbeddedSubEntity>? Embedded1 { get; set; }

            [Relations(["Duplicate"])]
            public IEmbeddedEntity<EmbeddedSubEntity2>? Embedded2 { get; set; }
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
