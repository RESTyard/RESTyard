using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        protected HttpContext FakeHttpContext;
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
        }

        protected void TestInitBase()
        {
            RouteRegister = new RouteRegister();
            RouteKeyFactory = new RouteKeyFactory(RouteRegister);
            RouteResolverFactory = new RegisterRouteResolverFactory(new HypermediaExtensionsOptions());

            var services = new ServiceCollection();
            services.AddSingleton<LinkGenerator, FakeLinkGenerator>();
            services.AddSingleton<IRouteRegister>(RouteRegister);
            services.AddSingleton<IRouteKeyFactory>(RouteKeyFactory);
            FakeHttpContext = new DefaultHttpContext()
            {
                RequestServices = services.BuildServiceProvider(),
            };

            RouteResolver = RouteResolverFactory.CreateRouteResolver(FakeHttpContext, TestUrlConfig);
            SirenConverter = CreateSirenConverter();
            SirenConverterNoNullProperties = CreateSirenConverter(new HypermediaConverterConfiguration{ WriteNullProperties = false });
        }

        protected SirenConverter CreateSirenConverter(HypermediaConverterConfiguration? configuration = null)
        {
            return new SirenConverter(RouteResolver, QueryStringBuilder, configuration);
        }

        public static void AssertClassName(JsonObject obj, string name)
        {
            Assert.IsTrue(obj["class"] is JsonArray);
            var classArray = obj["class"]!.AsArray();
            Assert.AreEqual(1, classArray.Count);
            Assert.IsTrue(classArray[0]!.ToString() == name);
        }

        public static void AssertHasOnlySelfLink(JsonObject obj, string routeName)
        {
            Assert.IsTrue(obj["links"] is JsonArray);
            var linksArray = obj["links"]!.AsArray();
            Assert.AreEqual(1, linksArray.Count);

            Assert.AreEqual(DefaultHypermediaRelations.Self, linksArray[0]!["rel"]![0]!.ToString());
            AssertRoute(linksArray[0]!["href"]!.ToString(), routeName);
        }

        public static void AssertHasNoLinks(JsonObject obj)
        {
            Assert.IsTrue(obj["links"] is JsonArray);
            var linksArray = obj["links"]!.AsArray();
            Assert.AreEqual(0, linksArray.Count);
        }

        public static void AssertEmptyActions(JsonObject obj)
        {
            Assert.IsTrue(obj["actions"] is JsonArray);
            var actionsArray = obj["actions"]!.AsArray();
            Assert.AreEqual(0, actionsArray.Count);
        }

        public static void AssertEmptyEntities(JsonObject obj)
        {
            Assert.IsTrue(obj["entities"] is JsonArray);
            var entitiesArray = obj["entities"]!.AsArray();
            Assert.AreEqual(0, entitiesArray.Count);
        }

        public static void AssertEmptyProperties(JsonObject siren)
        {
            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);
            Assert.AreEqual(0, propertiesObject.Count);
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

        public static void AssertHasLink(JsonArray linksArray, string linkRelation, string routeNameLinking)
        {
            AssertHasLinkWithKeyAndQuery(linksArray, new List<string> { linkRelation }, routeNameLinking);
        }

        public static void AssertHasLink(JsonArray linksArray, List<string> linkRelations, string routeNameLinking)
        {
            AssertHasLinkWithKeyAndQuery(linksArray, linkRelations, routeNameLinking);
        }

        public static void AssertHasLinkWithKey(JsonArray linksArray, string linkRelation, string routeNameLinking, string keyObjectString)
        {
            AssertHasLinkWithKeyAndQuery(linksArray, new List<string> { linkRelation }, routeNameLinking, keyObjectString);
        }

        public static void AssertHasLinkWithKey(JsonArray linksArray, List<string> linkRelations, string routeNameLinking, string keyObjectString)
        {
            AssertHasLinkWithKeyAndQuery(linksArray, linkRelations, routeNameLinking, keyObjectString);
        }

        public static void AssertHasLinkWithQuery(JsonArray linksArray, string linkRelation, string routeNameLinking, string queryString)
        {
            AssertHasLinkWithKeyAndQuery(linksArray, new List<string> { linkRelation }, routeNameLinking, null, queryString);
        }

        public static void AssertHasLinkWithQuery(JsonArray linksArray, List<string> linkRelations, string routeNameLinking, string queryString)
        {
            AssertHasLinkWithKeyAndQuery(linksArray, linkRelations, routeNameLinking, null, queryString);
        }

        public static void AssertHasLinkWithKeyAndQuery(JsonArray linksArray, string linkRelation, string routeNameLinking, string? keyObjectString = null, string? queryString = null)
        {
            AssertHasLinkWithKeyAndQuery(linksArray, new List<string> { linkRelation }, routeNameLinking, keyObjectString, queryString);
        }

        public static void AssertHasLinkWithKeyAndQuery(JsonArray linksArray, List<string> linkRelations, string routeNameLinking, string? keyObjectString = null, string? queryString = null)
        {
            var foundLink = false;
            foreach (var link in linksArray)
            {
                if (!(link is JsonObject linkObject))
                {
                    throw new Exception("Link array item should be a JsonObject");
                }

                var relationArray = linkObject["rel"]!.AsArray();
                var sirenRelations = relationArray.Select(n => n!.GetValue<string>()).ToList();
                var hasDesiredRelations = StringReadOnlyListComparer.Equals(sirenRelations, linkRelations);

                if (hasDesiredRelations)
                {
                    AssertRoute(linkObject["href"]!.GetValue<string>(), routeNameLinking, keyObjectString, queryString);

                    foundLink = true;
                    break;
                }
            }

            Assert.IsTrue(foundLink);
        }
    }
}
