using System;
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


            var siren = SirenBuilder.CreateSirenJObject(ho, RouteResolver, QueryStringBuilder);

            AssertDefaultClassName(siren, typeof(LinkingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, ho.Links.Count);

            AssertHasLink(linksArray, DefaultHypermediaRelations.Self, routeNameLinking, true);
            AssertHasLink(linksArray, link1Rel, routeNameLinked1, true);
            AssertHasLink(linksArray, link2Rel, routeNameLinked2, true);
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

            var siren = SirenBuilder.CreateSirenJObject(ho, RouteResolver, QueryStringBuilder);

            AssertDefaultClassName(siren, typeof(LinkingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, ho.Links.Count);

            AssertHasLink(linksArray, DefaultHypermediaRelations.Self, routeNameLinking, true);
            AssertHasLinkWithKey(linksArray, link1Rel, routeNameLinked1, "{ key = 42 }", true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
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

            var siren = SirenBuilder.CreateSirenJObject(ho, RouteResolver, QueryStringBuilder);
        }

        [TestMethod]
        public void LinksHypermediaObjectKeyReferenceTest()
        {
            var routeNameLinking = typeof(LinkingHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(LinkingHypermediaObject), routeNameLinking);

            var routeNameLinked1 = typeof(Linked1HypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(Linked1HypermediaObject), routeNameLinked1);
            var link1Rel = "Link1";

            var routeNameLinked2 = typeof(Linked2HypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(Linked2HypermediaObject), routeNameLinked2);
            var link2Rel = "Link2";

            var ho = new LinkingHypermediaObject();
            ho.Links.Add(link1Rel, new HypermediaObjectKeyReference(typeof(Linked1HypermediaObject), 5));
            ho.Links.Add(link2Rel, new HypermediaObjectKeyReference(typeof(Linked2HypermediaObject), "AStringkey"));

            var siren = SirenBuilder.CreateSirenJObject(ho, RouteResolver, QueryStringBuilder);

            AssertDefaultClassName(siren, typeof(LinkingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, ho.Links.Count);

            AssertHasLink(linksArray, DefaultHypermediaRelations.Self, routeNameLinking, true);
            AssertHasLinkWithKey(linksArray, link1Rel, routeNameLinked1, "{ key = 5 }", true);
            AssertHasLinkWithKey(linksArray, link2Rel, routeNameLinked2, "{ key = AStringkey }", true);
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

            var siren = SirenBuilder.CreateSirenJObject(ho, RouteResolver, QueryStringBuilder);

            AssertDefaultClassName(siren, typeof(LinkingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, ho.Links.Count);

            AssertHasLink(linksArray, DefaultHypermediaRelations.Self, routeNameLinking, true);
            AssertHasLinkWithQuery(linksArray, link1Rel, routeNameLinked1, QueryStringBuilder.CreateQueryString(queryObject1), true);
            AssertHasLinkWithKeyAndQuery(linksArray, link2Rel, routeNameLinked2, "{ key = 3 }", QueryStringBuilder.CreateQueryString(queryObject2),true);
        }

    }

    public class LinkingHypermediaObject : HypermediaObject
    {
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

    public class LinkedHypermediaObjectWithKeyRouteKeyProvider : IRouteKeyProducer
    {
        public object GetKey(object hypermediaObject)
        {
            var ho = (LinkedHypermediaObjectWithKey) hypermediaObject;
            return new {key = ho.Id};
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
