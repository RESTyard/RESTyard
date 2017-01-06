using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Query;
using WebApiHypermediaExtensionsCore.WebApi;
using WebApiHypermediaExtensionsCore.WebApi.AttributedRoutes;
using WebApiHypermediaExtensionsCore.WebApi.Formatter;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace WebApiHypermediaExtensionsCore.Test.WebApi.Formatter
{
    public class SirenBuilderTestBase
    {
        protected static QueryStringBuilder QueryStringBuilder;
        protected static HypermediaUrlConfig TestUrlConfig;
        protected static MockUrlHelper UrlHelper;
        protected RouteRegister RouteRegister;
        protected RegisterRouteResolverFactory RouteResolverFactory;
        protected RouteKeyFactory RouteKeyFactory;
        protected SirenBuilder SirenBuilder;
        protected IHypermediaRouteResolver RouteResolver;

        protected static void ClassInitBase()
        {
            QueryStringBuilder = new QueryStringBuilder();

            TestUrlConfig = new HypermediaUrlConfig()
            {
                Host = new HostString("MyHost", 1234),
                Scheme = "Scheme"
            };

            UrlHelper = new MockUrlHelper();
        }

        protected void TestInitBase()
        {
            RouteRegister = new RouteRegister();
            RouteResolverFactory = new RegisterRouteResolverFactory(RouteRegister);
            RouteKeyFactory = new RouteKeyFactory(RouteRegister);

            SirenBuilder = new SirenBuilder();

            RouteResolver = RouteResolverFactory.CreateRouteResolver(UrlHelper, RouteKeyFactory, TestUrlConfig);
        }

        public static void AssertDefaultClassName(JObject obj, Type type)
        {
            Assert.IsTrue(obj["class"].Type == JTokenType.Array);
            var classArray = (JArray)obj["class"];
            Assert.AreEqual(classArray.Count, 1);
            Assert.IsTrue(obj["class"].First.ToString() == type.Name);
        }

        public static void AssertHasOnlySelfLink(JObject obj, string routeName)
        {
            Assert.IsTrue(obj["links"].Type == JTokenType.Array);
            var linksArray = (JArray)obj["links"];
            Assert.AreEqual(linksArray.Count, 1);

            Assert.AreEqual(obj["links"].First["rel"].First.ToString(), DefaultHypermediaRelations.Self);
            AssertRoute(obj["links"].First["href"].ToString(), routeName);
        }

        public static void AssertEmptyActions(JObject obj)
        {
            Assert.IsTrue(obj["actions"].Type == JTokenType.Array);
            var actionsArray = (JArray)obj["actions"];
            Assert.AreEqual(actionsArray.Count, 0);
        }

        public static void AssertEmptyEntities(JObject obj)
        {
            Assert.IsTrue(obj["entities"].Type == JTokenType.Array);
            var entitiesArray = (JArray)obj["entities"];
            Assert.AreEqual(entitiesArray.Count, 0);
        }

        public static void AssertEmptyProperties(JObject siren)
        {
            Assert.IsTrue(siren["properties"].Type == JTokenType.Object);
            var propertiesObject = (JObject)siren["properties"];
            Assert.AreEqual(propertiesObject.Properties().Count(), 0);
        }

        public static void AssertRoute(string route, string expectedRouteName, string keyObjectString = null, string queryString = null)
        {
            var segments = route.Split('/');
            Assert.AreEqual(segments[0], TestUrlConfig.Scheme);
            Assert.AreEqual(segments[1], TestUrlConfig.Host.ToString());
            Assert.AreEqual(segments[2], expectedRouteName);

            if (keyObjectString != null && queryString != null)
            {
                Assert.AreEqual(segments[3], keyObjectString+queryString);
            }
            else if (queryString != null)
            {
                Assert.AreEqual(segments[3], queryString);
            }
            else if (keyObjectString != null)
            {
                Assert.AreEqual(segments[3], keyObjectString);
            }
        }

        public static void AssertHasLink(JArray linksArray, string linkName, string routeNameLinking, bool noOtherRelation = false)
        {
            AssertHasLinkWithKeyAndQuery(linksArray, linkName, routeNameLinking, null, null, noOtherRelation);
        }

        public static void AssertHasLinkWithKey(JArray linksArray, string linkName, string routeNameLinking, string keyObjectString, bool noOtherRelation = false)
        {
            AssertHasLinkWithKeyAndQuery(linksArray, linkName, routeNameLinking, keyObjectString, null, noOtherRelation);
        }

        public static void AssertHasLinkWithQuery(JArray linksArray, string linkName, string routeNameLinking, string queryString, bool noOtherRelation = false)
        {
            AssertHasLinkWithKeyAndQuery(linksArray, linkName, routeNameLinking, null, queryString, noOtherRelation);
        }

        public static void AssertHasLinkWithKeyAndQuery(JArray linksArray, string linkName, string routeNameLinking, string keyObjectString = null, string queryString = null, bool noOtherRelation = false)
        {
            var foundLink = false;
            foreach (var link in linksArray)
            {
                var linkObject = link as JObject;
                if (linkObject == null)
                {
                    throw new Exception("Link array item should be a JObject");
                }

                var relationArray = (JArray)linkObject["rel"];
                var hasDesiredLink = relationArray.FirstOrDefault(i => i.Value<string>().Equals(linkName)) != null;
                if (noOtherRelation)
                {
                    Assert.AreEqual(relationArray.Count, 1);
                }

                if (hasDesiredLink)
                {
                    AssertRoute(((JValue)linkObject["href"]).Value<string>(), routeNameLinking, keyObjectString, queryString);

                    foundLink = true;
                    break;
                }
            }

            Assert.IsTrue(foundLink);
        }
    }
}