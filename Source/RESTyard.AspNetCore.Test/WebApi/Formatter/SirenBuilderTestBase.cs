using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
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
            var classArray = obj["class"].Should().BeOfType<JsonArray>().Which;
            classArray.Should().ContainSingle().Which!.ToString().Should().Be(name);
        }

        public static void AssertHasOnlySelfLink(JsonObject obj, string routeName)
        {
            var linksArray = obj["links"].Should().BeOfType<JsonArray>().Which;
            linksArray.Should().ContainSingle().Which!["rel"]![0]!.ToString().Should().Be(DefaultHypermediaRelations.Self);
            AssertRoute(linksArray[0]!["href"]!.ToString(), routeName);
        }

        public static void AssertHasNoLinks(JsonObject obj)
        {
            var linksArray = obj["links"].Should().BeOfType<JsonArray>().Which;
            linksArray.Should().BeEmpty();
        }

        public static void AssertEmptyActions(JsonObject obj)
        {
            var actionsArray = obj["actions"].Should().BeOfType<JsonArray>().Which;
            actionsArray.Should().BeEmpty();
        }

        public static void AssertEmptyEntities(JsonObject obj)
        {
            var entitiesArray = obj["entities"].Should().BeOfType<JsonArray>().Which;
            entitiesArray.Should().BeEmpty();
        }

        public static void AssertEmptyProperties(JsonObject siren)
        {
            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);
            propertiesObject.Should().BeEmpty();
        }

        public static void AssertRoute(string route, string expectedRouteName, string? keyObjectString = null, string? queryString = null)
        {
            var segments = route.Split('/', '?');
            var routAsUri = new Uri(route);
            routAsUri.Scheme.Should().Be(TestUrlConfig.Scheme);
            TestUrlConfig.Host.ToString().Should().Be(routAsUri.Host + ":" + routAsUri.Port);
            GetPathWithoutQuery(routAsUri).Should().Be(expectedRouteName);

            if (keyObjectString != null && queryString != null)
            {
                segments[4].Should().Be(keyObjectString);
                ("?" + segments[5]).Should().Be(queryString);
            }
            else if (queryString != null)
            {
                ("?" + segments[4]).Should().Be(queryString);
            }
            else if (keyObjectString != null)
            {
                segments[4].Should().Be(keyObjectString);
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

            foundLink.Should().BeTrue();
        }
    }
}
