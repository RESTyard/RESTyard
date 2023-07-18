using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.Test.WebApi.Formatter.Properties;
using RESTyard.AspNetCore.Util;
using RESTyard.AspNetCore.WebApi;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;
using RESTyard.AspNetCore.WebApi.Formatter;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using RESTyard.Relations;

namespace RESTyard.AspNetCore.Test.WebApi.Formatter
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
        protected SirenConverter SirenConverterNoNullProperties;
        protected IHypermediaRouteResolver RouteResolver;
        private static readonly StringReadOnlyCollectionComparer StringReadOnlyListComparer = new StringReadOnlyCollectionComparer();

        protected static void ClassInitBase()
        {
            QueryStringBuilder = new QueryStringBuilder();

            TestUrlConfig = new HypermediaUrlConfig()
            {
                Host = new HostString("myhost", 1234),
                Scheme = "scheme"
            };

            UrlHelper = new MockUrlHelper();
        }

        protected void TestInitBase()
        {
            RouteRegister = new RouteRegister();
            RouteKeyFactory = new RouteKeyFactory(RouteRegister);
            RouteResolverFactory = new RegisterRouteResolverFactory(RouteRegister, new HypermediaExtensionsOptions(), RouteKeyFactory, TestUrlConfig);

            RouteResolver = RouteResolverFactory.CreateRouteResolver(UrlHelper);
            SirenConverter = CreateSirenConverter();
            SirenConverterNoNullProperties = CreateSirenConverter(new HypermediaConverterConfiguration{ WriteNullProperties = false });
        }

        protected SirenConverter CreateSirenConverter(HypermediaConverterConfiguration configuration = null)
        {
            return new SirenConverter(RouteResolver, QueryStringBuilder, configuration);
        }

        public static void AssertDefaultClassName(JObject obj, Type type)
        {
            Assert.IsTrue(obj["class"].Type == JTokenType.Array);
            var classArray = (JArray)obj["class"];
            Assert.AreEqual(1, classArray.Count);
            Assert.IsTrue(obj["class"].First.ToString() == type.BeautifulName());
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
            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);
            Assert.AreEqual(0, propertiesObject.Properties().Count());
        }

        public static void AssertRoute(string route, string expectedRouteName, string? keyObjectString = null, string? queryString = null)
        {
            var segments = route.Split('/', '?');
            var routAsUri = new Uri(route);
            Assert.AreEqual(TestUrlConfig.Scheme,routAsUri.Scheme);
            Assert.AreEqual(TestUrlConfig.Host.ToString(), routAsUri.Host + ":" + routAsUri.Port);
            Assert.AreEqual(expectedRouteName, GetPathWithoutQuery(routAsUri));

            if (keyObjectString != null && queryString != null)
            {
                Assert.AreEqual(keyObjectString, segments[4]);
                Assert.AreEqual(queryString, "?" + segments[5]);
            }
            else if (queryString != null)
            {
                Assert.AreEqual(queryString, "?" + segments[4]);
            }
            else if (keyObjectString != null)
            {
                Assert.AreEqual(keyObjectString, segments[4]);
            }
        }

        private static string GetPathWithoutQuery(Uri routAsUri)
        {
            var indexOfQuery = routAsUri.AbsolutePath.IndexOf("?", StringComparison.Ordinal);
            var indexOfKeyObject = routAsUri.AbsolutePath.IndexOf("%", StringComparison.Ordinal)-2;
            var cutOfIndex = Math.Max(indexOfQuery, indexOfKeyObject);
            return routAsUri.AbsolutePath.Substring(1,  cutOfIndex > 0 ? cutOfIndex : routAsUri.AbsolutePath.Length-1);
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
                if (!(link is JObject linkObject))
                {
                    throw new Exception("Link array item should be a JObject");
                }

                var relationArray = (JArray)linkObject["rel"];
                var sirenRelations = relationArray.Values<string>().ToList();
                var hasDesiredRelations = StringReadOnlyListComparer.Equals(sirenRelations, linkRelations);

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