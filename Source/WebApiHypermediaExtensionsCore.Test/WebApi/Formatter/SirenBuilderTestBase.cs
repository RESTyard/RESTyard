using System;
using System.Collections.Generic;
using System.Linq;
using Hypermedia.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
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
        protected SirenConverter SirenConverter;
        protected IHypermediaRouteResolver RouteResolver;
        private static readonly StringCollectionComparer stringListComparer = new StringCollectionComparer();

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

            RouteResolver = RouteResolverFactory.CreateRouteResolver(UrlHelper, RouteKeyFactory, TestUrlConfig);
            SirenConverter = CreateSirenConverter();
        }

        protected SirenConverter CreateSirenConverter(ISirenConverterConfiguration configuration = null)
        {
            return new SirenConverter(RouteResolver, QueryStringBuilder, configuration);
        }

        public static void AssertDefaultClassName(JObject obj, Type type)
        {
            Assert.IsTrue(obj["class"].Type == JTokenType.Array);
            var classArray = (JArray)obj["class"];
            Assert.AreEqual(1, classArray.Count);
            Assert.IsTrue(obj["class"].First.ToString() == type.Name);
        }

        public static void AssertHasOnlySelfLink(JObject obj, string routeName)
        {
            Assert.IsTrue(obj["links"].Type == JTokenType.Array);
            var linksArray = (JArray)obj["links"];
            Assert.AreEqual(1, linksArray.Count);

            Assert.AreEqual(DefaultHypermediaRelations.Self, obj["links"].First["rel"].First.ToString());
            AssertRoute(obj["links"].First["href"].ToString(), routeName);
        }

        public static void AssertEmptyActions(JObject obj)
        {
            Assert.IsTrue(obj["actions"].Type == JTokenType.Array);
            var actionsArray = (JArray)obj["actions"];
            Assert.AreEqual(0, actionsArray.Count);
        }

        public static void AssertEmptyEntities(JObject obj)
        {
            Assert.IsTrue(obj["entities"].Type == JTokenType.Array);
            var entitiesArray = (JArray)obj["entities"];
            Assert.AreEqual(0, entitiesArray.Count);
        }

        public static void AssertEmptyProperties(JObject siren)
        {
            Assert.IsTrue(siren["properties"].Type == JTokenType.Object);
            var propertiesObject = (JObject)siren["properties"];
            Assert.AreEqual(0, propertiesObject.Properties().Count());
        }

        public static void AssertRoute(string route, string expectedRouteName, string keyObjectString = null, string queryString = null)
        {
            var segments = route.Split('/', '?');
            Assert.AreEqual(TestUrlConfig.Scheme, segments[0]);
            Assert.AreEqual(TestUrlConfig.Host.ToString(), segments[1]);
            Assert.AreEqual(expectedRouteName, segments[2]);

            if (keyObjectString != null && queryString != null)
            {
                Assert.AreEqual(keyObjectString, segments[3]);
                Assert.AreEqual(queryString, "?" + segments[4]);
            }
            else if (queryString != null)
            {
                Assert.AreEqual(queryString, "?" + segments[3]);
            }
            else if (keyObjectString != null)
            {
                Assert.AreEqual(keyObjectString, segments[3]);
            }
        }

        public static void AssertHasLink(JArray linksArray, string linkRelation, string routeNameLinking)
        {
            AssertHasLinkWithKeyAndQuery(linksArray, new List<string> { linkRelation }, routeNameLinking);
        }

        public static void AssertHasLink(JArray linksArray, List<string> linkRelations, string routeNameLinking)
        {
            AssertHasLinkWithKeyAndQuery(linksArray, linkRelations, routeNameLinking);
        }

        public static void AssertHasLinkWithKey(JArray linksArray, string linkRelation, string routeNameLinking, string keyObjectString)
        {
            AssertHasLinkWithKeyAndQuery(linksArray, new List<string> { linkRelation }, routeNameLinking, keyObjectString);
        }

        public static void AssertHasLinkWithKey(JArray linksArray, List<string> linkRelations, string routeNameLinking, string keyObjectString)
        {
            AssertHasLinkWithKeyAndQuery(linksArray, linkRelations, routeNameLinking, keyObjectString);
        }

        public static void AssertHasLinkWithQuery(JArray linksArray, string linkRelation, string routeNameLinking, string queryString)
        {
            AssertHasLinkWithKeyAndQuery(linksArray, new List<string> { linkRelation }, routeNameLinking, null, queryString);
        }

        public static void AssertHasLinkWithQuery(JArray linksArray, List<string> linkRelations, string routeNameLinking, string queryString)
        {
            AssertHasLinkWithKeyAndQuery(linksArray, linkRelations, routeNameLinking, null, queryString);
        }

        public static void AssertHasLinkWithKeyAndQuery(JArray linksArray, string linkRelation, string routeNameLinking, string keyObjectString = null, string queryString = null)
        {
            AssertHasLinkWithKeyAndQuery(linksArray, new List<string> { linkRelation }, routeNameLinking, keyObjectString, queryString);
        }

        public static void AssertHasLinkWithKeyAndQuery(JArray linksArray, List<string> linkRelations, string routeNameLinking, string keyObjectString = null, string queryString = null)
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
                var sirenRelations = relationArray.Values<string>().ToList();
                var hasDesiredRelations = stringListComparer.Equals(sirenRelations, linkRelations);

                if (hasDesiredRelations)
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