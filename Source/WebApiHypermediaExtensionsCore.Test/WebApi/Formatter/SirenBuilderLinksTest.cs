using System;
using System.Collections.Generic;
using System.Linq;
using Hypermedia.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;
using WebApiHypermediaExtensionsCore.Query;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace WebApiHypermediaExtensionsCore.Test.WebApi.Formatter
{
    [TestClass]
    public class SirenBuilderLinksTest : SirenBuilderTestBase
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
        public void LinksHypermediaObjectNoSelfLinkTest()
        {
            var ho = new NoSelfLinkHypermediaObject();
            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(NoSelfLinkHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, ho.Links.Count);
        }

        [TestMethod]
        public void LinksHypermediaObjectReferenceTest()
        {
            var routeNameLinking = typeof(LinkingHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(LinkingHypermediaObject), routeNameLinking);

            var routeNameLinked1 = typeof(Linked1HypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(Linked1HypermediaObject), routeNameLinked1);
            var hoLink1 = new Linked1HypermediaObject();
            var link1Rel = "Link1";

            var routeNameLinked2 = typeof(Linked2HypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(Linked2HypermediaObject), routeNameLinked2);
            var hoLink2 = new Linked2HypermediaObject();
            var link2Rel = "Link2";

            var ho = new LinkingHypermediaObject();
            ho.Links.Add(link1Rel, new HypermediaObjectReference(hoLink1));
            ho.Links.Add(link2Rel, new HypermediaObjectReference(hoLink2));


            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(LinkingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, ho.Links.Count);

            AssertHasLink(linksArray, DefaultHypermediaRelations.Self, routeNameLinking);
            AssertHasLink(linksArray, link1Rel, routeNameLinked1);
            AssertHasLink(linksArray, link2Rel, routeNameLinked2);
        }

        [TestMethod]
        public void LinksHypermediaObjectReferenceWithRouteKeyTest()
        {
            var routeNameLinking = typeof(LinkingHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(LinkingHypermediaObject), routeNameLinking);

            var routeNameLinked1 = typeof(LinkedHypermediaObjectWithKey).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(LinkedHypermediaObjectWithKey), routeNameLinked1);
            var hoLink1 = new LinkedHypermediaObjectWithKey {Id = 42};
            var link1Rel = "Link1";
            RouteRegister.AddRouteKeyProducer(typeof(LinkedHypermediaObjectWithKey), new LinkedHypermediaObjectWithKeyRouteKeyProvider());

            var ho = new LinkingHypermediaObject();
            ho.Links.Add(link1Rel, new HypermediaObjectReference(hoLink1));

            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(LinkingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, ho.Links.Count);

            AssertHasLink(linksArray, DefaultHypermediaRelations.Self, routeNameLinking);
            AssertHasLinkWithKey(linksArray, link1Rel, routeNameLinked1, "{ key = 42 }");
        }

        [TestMethod]
        public void LinksDuplicateRelationTest()
        {
            var routeNameLinking = typeof(LinkingHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(LinkingHypermediaObject), routeNameLinking);

            var routeNameLinked1 = typeof(Linked1HypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(Linked1HypermediaObject), routeNameLinked1);
            var hoLink1 = new Linked1HypermediaObject();
            var duplicateRel = "Duplicate";

            var routeNameLinked2 = typeof(Linked2HypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(Linked2HypermediaObject), routeNameLinked2);
            var hoLink2 = new Linked2HypermediaObject();

            var ho = new LinkingHypermediaObject();
            ho.Links.Add(duplicateRel, new HypermediaObjectReference(hoLink1));
            ho.Links.Add(duplicateRel, new HypermediaObjectReference(hoLink2));

            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(LinkingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, ho.Links.Count);

            AssertHasLink(linksArray, duplicateRel, routeNameLinked2);
        }

        [TestMethod]
        public void LinksMultipleRelationTest()
        {
            var routeNameLinking = typeof(LinkingHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(LinkingHypermediaObject), routeNameLinking);

            var routeNameLinked1 = typeof(Linked1HypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(Linked1HypermediaObject), routeNameLinked1);
            var hoLink1 = new Linked1HypermediaObject();
            var multiRel = new List<string> {"RelA", "RelB"};

            var ho = new LinkingHypermediaObject();
            ho.Links.Add(multiRel, new HypermediaObjectReference(hoLink1));

            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(LinkingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, ho.Links.Count);

            AssertHasLink(linksArray, multiRel, routeNameLinked1);
        }

        [TestMethod]
        public void LinksHypermediaObjectKeyReferenceTest()
        {
            var routeNameLinking = typeof(LinkingHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(LinkingHypermediaObject), routeNameLinking);

            var routeNameLinked1 = typeof(Linked1HypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(Linked1HypermediaObject), routeNameLinked1);
            RouteRegister.AddRouteKeyProducer(typeof(Linked1HypermediaObject), new Linked1HypermediaObjectRouteKeyProducer());
            var link1Rel = "Link1";

            var routeNameLinked2 = typeof(Linked2HypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(Linked2HypermediaObject), routeNameLinked2);
            RouteRegister.AddRouteKeyProducer(typeof(Linked2HypermediaObject), new Linked2HypermediaObjectRouteKeyProducer());
            var link2Rel = "Link2";

            var ho = new LinkingHypermediaObject();
            ho.Links.Add(link1Rel, new HypermediaObjectKeyReference(typeof(Linked1HypermediaObject), 5));
            ho.Links.Add(link2Rel, new HypermediaObjectKeyReference(typeof(Linked2HypermediaObject), "AStringkey"));

            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(LinkingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, ho.Links.Count);

            AssertHasLink(linksArray, DefaultHypermediaRelations.Self, routeNameLinking);
            AssertHasLinkWithKey(linksArray, link1Rel, routeNameLinked1, "{ key = 5 }");
            AssertHasLinkWithKey(linksArray, link2Rel, routeNameLinked2, "{ key = AStringkey }");
        }

        [TestMethod]
        public void LinksHypermediaObjectQueryReferenceTest()
        {
            var routeNameLinking = typeof(LinkingHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(LinkingHypermediaObject), routeNameLinking);

            var routeNameLinked1 = typeof(Linked1HypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(Linked1HypermediaObject), routeNameLinked1);
            var link1Rel = "Link1";
            var queryObject1 = new QueryObject {ABool = true, AInt = 3};
            

            var routeNameLinked2 = typeof(LinkedHypermediaObjectWithKey).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(LinkedHypermediaObjectWithKey), routeNameLinked2);
            RouteRegister.AddRouteKeyProducer(typeof(LinkedHypermediaObjectWithKey), new LinkedHypermediaObjectWithKeyRouteKeyProvider());
            var link2Rel = "Link2";
            var queryObject2 = new QueryObject { ABool = false, AInt = 5 };

            var ho = new LinkingHypermediaObject();
            ho.Links.Add(link1Rel, new HypermediaObjectQueryReference(typeof(Linked1HypermediaObject), queryObject1));
            ho.Links.Add(link2Rel, new HypermediaObjectQueryReference(typeof(LinkedHypermediaObjectWithKey), queryObject2, 3));

            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(LinkingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, ho.Links.Count);

            AssertHasLink(linksArray, DefaultHypermediaRelations.Self, routeNameLinking);
            AssertHasLinkWithQuery(linksArray, link1Rel, routeNameLinked1, QueryStringBuilder.CreateQueryString(queryObject1));
            AssertHasLinkWithKeyAndQuery(linksArray, link2Rel, routeNameLinked2, "{ key = 3 }", QueryStringBuilder.CreateQueryString(queryObject2));
        }


        [TestMethod]
        public void LinksExternalReferenceTest()
        {
            var externalUri = "http://www.example.com/";
            var routeNameLinking = typeof(ExternalUsingHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(ExternalUsingHypermediaObject), routeNameLinking);

            var ho = new ExternalUsingHypermediaObject();
            const string rel = "External";
            ho.Links.Add(rel, new ExternalReference(new Uri(externalUri)));

            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(ExternalUsingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, ho.Links.Count);

            var linkObject = linksArray[0] as JObject;
            if (linkObject == null)
            {
                throw new Exception("Link array item should be a JObject");
            }

            var relationArray = (JArray)linkObject["rel"];
            var sirenRelations = relationArray.Values<string>().ToList();
            var stringListComparer = new StringCollectionComparer();
            var hasDesiredRelations = stringListComparer.Equals(sirenRelations, new List<string> {rel});

            Assert.IsTrue(hasDesiredRelations);
            Assert.AreEqual(((JValue)linkObject["href"]).Value<string>(), externalUri);

        }
    }

    public class Linked1HypermediaObjectRouteKeyProducer : IKeyProducer
    {
        public object CreateFromHypermediaObject(HypermediaObject hypermediaObject)
        {
            throw new NotImplementedException();
        }

        public object CreateFromKeyObject(object keyObject)
        {
            return new {key = keyObject};
        }
    }

    public class Linked2HypermediaObjectRouteKeyProducer : IKeyProducer
    {
        public object CreateFromHypermediaObject(HypermediaObject hypermediaObject)
        {
            throw new NotImplementedException();
        }

        public object CreateFromKeyObject(object keyObject)
        {
            return new { key = keyObject };
        }
    }

    public class NoSelfLinkHypermediaObject : HypermediaObject
    {
        public NoSelfLinkHypermediaObject() :base(false)
        {
        }
    }

    public class LinkingHypermediaObject : HypermediaObject
    {
    }

    public class ExternalUsingHypermediaObject : HypermediaObject
    {
        public ExternalUsingHypermediaObject() : base(false)
        {
        }
    }

    public class Linked1HypermediaObject : HypermediaObject
    {
    }

    public class Linked2HypermediaObject : HypermediaObject
    {
    }

    public class Linked3HypermediaObject : HypermediaObject
    {
    }

    public class LinkedHypermediaObjectWithKeyRouteKeyProvider : IKeyProducer
    {
        public object CreateFromHypermediaObject(HypermediaObject hypermediaObject)
        {
            var ho = (LinkedHypermediaObjectWithKey) hypermediaObject;
            return new {key = ho.Id};
        }

       public object CreateFromKeyObject(object keyObject)
       {
           return new {key = keyObject};
       }
    }

    public class LinkedHypermediaObjectWithKey : HypermediaObject
    {
        public  int Id { get; set; }
    }

    public class QueryObject : IHypermediaQuery
    {
        public bool ABool { get; set; }
        public int AInt { get; set; }
    }


}
