using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.Util;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using RESTyard.Relations;

namespace RESTyard.AspNetCore.Test.WebApi.Formatter
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

            AssertClassName(siren, nameof(NoSelfLinkHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, 0);
        }

        [TestMethod]
        public void LinksHypermediaObjectReferenceTest()
        {
            var routeNameLinking = nameof(LinkingHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(LinkingHypermediaObject), routeNameLinking, HttpMethod.GET);

            var routeNameLinked1 = nameof(Linked1HypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(Linked1HypermediaObject), routeNameLinked1, HttpMethod.GET);
            var hoLink1 = new Linked1HypermediaObject();
            var link1Rel = "Link1";

            var routeNameLinked2 = nameof(Linked2HypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(Linked2HypermediaObject), routeNameLinked2, HttpMethod.GET);
            var hoLink2 = new Linked2HypermediaObject();
            var link2Rel = "Link2";

            var ho = new LinkingHypermediaObject();
            ho.Link1 = new(new HypermediaObjectReference(hoLink1));
            ho.Link2 = new(new HypermediaObjectReference(hoLink2));


            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(LinkingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, 3);

            AssertHasLink(linksArray, DefaultHypermediaRelations.Self, routeNameLinking);
            AssertHasLink(linksArray, link1Rel, routeNameLinked1);
            AssertHasLink(linksArray, link2Rel, routeNameLinked2);
        }

        [TestMethod]
        public void LinksHypermediaObjectReferenceWithRouteKeyTest()
        {
            var routeNameLinking = nameof(LinkingHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(LinkingHypermediaObject), routeNameLinking, HttpMethod.GET);

            var routeNameLinked1 = nameof(LinkedHypermediaObjectWithKey) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(LinkedHypermediaObjectWithKey), routeNameLinked1, HttpMethod.GET);
            var hoLink1 = new LinkedHypermediaObjectWithKey {Id = 42};
            var link1Rel = "Link1";
            RouteRegister.AddRouteKeyProducer(typeof(LinkedHypermediaObjectWithKey), new LinkedHypermediaObjectWithKeyRouteKeyProvider());

            var ho = new LinkingHypermediaObject();
            ho.Link1 = new(new HypermediaObjectReference(hoLink1));

            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(LinkingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, 2);

            AssertHasLink(linksArray, DefaultHypermediaRelations.Self, routeNameLinking);
            AssertHasLinkWithKey(linksArray, link1Rel, routeNameLinked1, "{ key = 42 }");
        }

        [TestMethod]
        public void LinksDuplicateRelationTest()
        {
            var routeNameLinking = nameof(LinkingHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(LinkingHypermediaObject), routeNameLinking, HttpMethod.GET);

            var routeNameLinked1 = nameof(Linked1HypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(Linked1HypermediaObject), routeNameLinked1, HttpMethod.GET);
            var hoLink1 = new Linked1HypermediaObject();
            var duplicateRel = "Duplicate";

            var routeNameLinked2 = nameof(Linked2HypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(Linked2HypermediaObject), routeNameLinked2, HttpMethod.GET);
            var hoLink2 = new Linked2HypermediaObject();

            var ho = new DuplicateLinkingHypermediaObject();
            ho.Link1 = new(new HypermediaObjectReference(hoLink1));
            ho.Link2 = new(new HypermediaObjectReference(hoLink2));

            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(DuplicateLinkingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, 2);

            AssertHasLink(linksArray, duplicateRel, routeNameLinked2);
        }

        [TestMethod]
        public void LinksMultipleRelationTest()
        {
            var routeNameLinking = nameof(LinkingHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(LinkingHypermediaObject), routeNameLinking, HttpMethod.GET);

            var routeNameLinked1 = nameof(Linked1HypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(Linked1HypermediaObject), routeNameLinked1, HttpMethod.GET);
            var hoLink1 = new Linked1HypermediaObject();
            var multiRel = new List<string> {"RelA", "RelB"};

            var ho = new MultiRelLinkingHypermediaObject();
            ho.Link1 = new(new HypermediaObjectReference(hoLink1));

            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(MultiRelLinkingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, 2);

            AssertHasLink(linksArray, multiRel, routeNameLinked1);
        }

        [TestMethod]
        public void LinksHypermediaObjectKeyReferenceTest()
        {
            var routeNameLinking = nameof(LinkingHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(LinkingHypermediaObject), routeNameLinking, HttpMethod.GET);

            var routeNameLinked1 = nameof(Linked1HypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(Linked1HypermediaObject), routeNameLinked1, HttpMethod.GET);
            RouteRegister.AddRouteKeyProducer(typeof(Linked1HypermediaObject), new Linked1HypermediaObjectRouteKeyProducer());
            var link1Rel = "Link1";

            var routeNameLinked2 = nameof(Linked2HypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(Linked2HypermediaObject), routeNameLinked2, HttpMethod.GET);
            RouteRegister.AddRouteKeyProducer(typeof(Linked2HypermediaObject), new Linked2HypermediaObjectRouteKeyProducer());
            var link2Rel = "Link2";

            var ho = new LinkingHypermediaObject();
            ho.Link1 = new(new HypermediaObjectKeyReference(typeof(Linked1HypermediaObject), 5));
            ho.Link2 = new(new HypermediaObjectKeyReference(typeof(Linked2HypermediaObject), "AStringkey"));

            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(LinkingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, 3);

            AssertHasLink(linksArray, DefaultHypermediaRelations.Self, routeNameLinking);
            AssertHasLinkWithKey(linksArray, link1Rel, routeNameLinked1, "{ key = 5 }");
            AssertHasLinkWithKey(linksArray, link2Rel, routeNameLinked2, "{ key = AStringkey }");
        }

        [TestMethod]
        public void LinksHypermediaObjectQueryReferenceTest()
        {
            var routeNameLinking = nameof(LinkingHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(LinkingHypermediaObject), routeNameLinking, HttpMethod.GET);

            var routeNameLinked1 = nameof(Linked1HypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(Linked1HypermediaObject), routeNameLinked1, HttpMethod.GET);
            var link1Rel = "Link1";
            var queryObject1 = new QueryObject {ABool = true, AInt = 3};
            

            var routeNameLinked2 = nameof(LinkedHypermediaObjectWithKey) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(LinkedHypermediaObjectWithKey), routeNameLinked2, HttpMethod.GET);
            RouteRegister.AddRouteKeyProducer(typeof(LinkedHypermediaObjectWithKey), new LinkedHypermediaObjectWithKeyRouteKeyProvider());
            var link2Rel = "Link2";
            var queryObject2 = new QueryObject { ABool = false, AInt = 5 };

            var ho = new LinkingHypermediaObject();
            ho.Link1 = new(new HypermediaObjectQueryReference(typeof(Linked1HypermediaObject), queryObject1));
            ho.Link2 = new(new HypermediaObjectQueryReference(typeof(LinkedHypermediaObjectWithKey), queryObject2, 3));

            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(LinkingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, 3);

            AssertHasLink(linksArray, DefaultHypermediaRelations.Self, routeNameLinking);
            AssertHasLinkWithQuery(linksArray, link1Rel, routeNameLinked1, QueryStringBuilder.CreateQueryString(queryObject1));
            AssertHasLinkWithKeyAndQuery(linksArray, link2Rel, routeNameLinked2, "{ key = 3 }", QueryStringBuilder.CreateQueryString(queryObject2));
        }

        [TestMethod]
        public void LinksExternalReferenceTest()
        {
            var externalUri = "http://www.example.com/";
            var routeNameLinking = nameof(ExternalUsingHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(ExternalUsingHypermediaObject), routeNameLinking, HttpMethod.GET);

            var ho = new ExternalUsingHypermediaObject();
            var rels = new List<string>()
            {
                "External0",
                "External1",
                "External2",
                "External3",
            };
            
            var availableMediaType1 = "custom/mediatype";
            var availableMediaTypes2 = new List<string>{"custom/mediaType1", "custom/mediaType2"};
            var availableMediaTypes = new List<List<string>>
            {
                new List<string>(),
                new List<string> { availableMediaType1 },
                availableMediaTypes2,
                new List<string>()
            };
            
            ho.Link1 = new(new HypermediaObjectReference(new ExternalReference(new Uri(externalUri))));
            ho.Link2 = new(new HypermediaObjectReference(new ExternalReference(new Uri(externalUri)).WithAvailableMediaType(availableMediaType1)));
            ho.Link3 = new(new HypermediaObjectReference(new ExternalReference(new Uri(externalUri)).WithAvailableMediaTypes(availableMediaTypes2)));
            ho.Link4 = new(new HypermediaObjectReference(new ExternalReference(new Uri(externalUri)).WithAvailableMediaTypes([])));

            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(ExternalUsingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, 4);

            var i = 0;
            foreach (var jToken in linksArray)
            {
                if (!(linksArray[i] is JObject linkObject))
                {
                    throw new Exception("Link array item should be a JObject");
                }
                
                AssertLink(linkObject, rels[i], externalUri, availableMediaTypes[i]);
                i++;
            }
           
        }

        private static void AssertLink(JObject linkObject, string rel, string expectedUrl, IReadOnlyCollection<string> expectedAvailableMediaTypes)
        {
            var relationArray = (JArray)linkObject["rel"];
            var sirenRelations = relationArray.Values<string>().ToList();
            var stringListComparer = new StringReadOnlyCollectionComparer();
            var hasDesiredRelations = stringListComparer.Equals(sirenRelations, new List<string> { rel });

            Assert.IsTrue(hasDesiredRelations);
            Assert.AreEqual(expectedUrl, ((JValue)linkObject["href"]).Value<string>());

            var typeParameter = linkObject["type"];
            if (typeParameter == null)
            {
                Assert.IsTrue(expectedAvailableMediaTypes.Count == 0, $"Expected media types: {string.Join(",", expectedAvailableMediaTypes)}");
            }
            else
            {
                AssertMediaTypes(expectedAvailableMediaTypes, linkObject["type"]);
            }
        }
        
         [TestMethod]
        public void LinksInternalReferenceTest()
        {
            var routeNameLinking = nameof(InternalUsingHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(InternalUsingHypermediaObject), routeNameLinking, HttpMethod.GET);

            var ho = new InternalUsingHypermediaObject();
            var rels = new List<string>()
            {
                "External0",
                "External1",
                "External2",
                "External3",
            };

            var availableMediaType1 = "custom/mediatype";
            var availableMediaTypes2 = new List<string>{"custom/mediaType1", "custom/mediaType2"};
            var availableMediaTypes = new List<List<string>>
            {
                new List<string>(),
                new List<string> { availableMediaType1 },
                availableMediaTypes2,
                new List<string>()
            };

            var routeName = "ARouteName";
            ho.Link1 = new(new HypermediaObjectReference(new InternalReference(routeName)));
            ho.Link2 = new(new HypermediaObjectReference(new InternalReference(routeName).WithAvailableMediaType(availableMediaType1)));
            ho.Link3 = new(new HypermediaObjectReference(new InternalReference(routeName).WithAvailableMediaTypes(availableMediaTypes2)));
            ho.Link4 = new(new HypermediaObjectReference(new InternalReference(routeName).WithAvailableMediaTypes( Array.Empty<string>())));

            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(InternalUsingHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);

            Assert.IsTrue(siren["links"].Type == JTokenType.Array);
            var linksArray = (JArray)siren["links"];
            Assert.AreEqual(linksArray.Count, 4);

            var internalRoute = $"{TestUrlConfig.Scheme}://{TestUrlConfig.Host}/{routeName}";
            var i = 0;
            foreach (var jToken in linksArray)
            {
                if (!(linksArray[i] is JObject linkObject))
                {
                    throw new Exception("Link array item should be a JObject");
                }
                
                AssertLink(linkObject, rels[i], internalRoute, availableMediaTypes[i]);
                i++;
            }
           
        }

        private static void AssertMediaTypes(IReadOnlyCollection<string> expectedAvailableMediaType, JToken typeToken)
        {
            var mediaTypesFromJToken = typeToken.Value<string>().Split(',').ToList();
            var stringReadOnlyCollectionComparer = new StringReadOnlyCollectionComparer();
            var hasDesiredMediaTypes = stringReadOnlyCollectionComparer.Equals(expectedAvailableMediaType, mediaTypesFromJToken);

            Assert.IsTrue(hasDesiredMediaTypes, $"Expected media types do not match {string.Join(",", expectedAvailableMediaType)} <-> {string.Join(",", mediaTypesFromJToken)}");
        }
    }

    public class Linked1HypermediaObjectRouteKeyProducer : IKeyProducer
    {
        public object CreateFromHypermediaObject(IHypermediaObject hypermediaObject)
        {
            throw new NotImplementedException();
        }

        public object CreateFromKeyObject(object? keyObject)
        {
            return new {key = keyObject};
        }
    }

    public class Linked2HypermediaObjectRouteKeyProducer : IKeyProducer
    {
        public object CreateFromHypermediaObject(IHypermediaObject hypermediaObject)
        {
            throw new NotImplementedException();
        }

        public object CreateFromKeyObject(object? keyObject)
        {
            return new { key = keyObject };
        }
    }

    [HypermediaObject(Classes = [nameof(NoSelfLinkHypermediaObject)])]
    public class NoSelfLinkHypermediaObject : IHypermediaObject
    {
    }

    [HypermediaObject(Classes = [nameof(LinkingHypermediaObject)])]
    public class LinkingHypermediaObject : IHypermediaObject
    {
        [Relations(["Link1"])]
        public Link Link1 { get; set; }
        
        [Relations(["Link2"])]
        public Link Link2 { get; set; }

        [Relations([DefaultHypermediaRelations.Self])]
        public Link Self => new Link(new HypermediaObjectKeyReference(typeof(LinkingHypermediaObject)));
    }

    [HypermediaObject(Classes = [nameof(DuplicateLinkingHypermediaObject)])]
    public class DuplicateLinkingHypermediaObject : IHypermediaObject
    {
        [Relations(["Duplicate"])]
        public Link Link1 { get; set; }
        
        [Relations(["Duplicate"])]
        public Link Link2 { get; set; }

        [Relations([DefaultHypermediaRelations.Self])]
        public Link Self => new Link(new HypermediaObjectKeyReference(typeof(LinkingHypermediaObject)));
    }

    [HypermediaObject(Classes = [nameof(MultiRelLinkingHypermediaObject)])]
    public class MultiRelLinkingHypermediaObject : IHypermediaObject
    {
        [Relations(["RelA", "RelB"])]
        public Link Link1 { get; set; }

        [Relations([DefaultHypermediaRelations.Self])]
        public Link Self => new Link(new HypermediaObjectKeyReference(typeof(LinkingHypermediaObject)));
    }

    [HypermediaObject(Classes = [nameof(ExternalUsingHypermediaObject)])]
    public class ExternalUsingHypermediaObject : IHypermediaObject
    {
        [Relations(["External0"])]
        public Link Link1 { get; set; }
        
        [Relations(["External1"])]
        public Link Link2 { get; set; }
        
        [Relations(["External2"])]
        public Link Link3 { get; set; }
        
        [Relations(["External3"])]
        public Link Link4 { get; set; }
    }
    
    [HypermediaObject(Classes = [nameof(InternalUsingHypermediaObject)])]
    public class InternalUsingHypermediaObject : IHypermediaObject
    {
        [Relations(["External0"])]
        public Link Link1 { get; set; }
        
        [Relations(["External1"])]
        public Link Link2 { get; set; }
        
        [Relations(["External2"])]
        public Link Link3 { get; set; }
        
        [Relations(["External3"])]
        public Link Link4 { get; set; }
    }

    [HypermediaObject(Classes = [nameof(Linked1HypermediaObject)])]
    public class Linked1HypermediaObject : IHypermediaObject
    {
    }

    [HypermediaObject(Classes = [nameof(Linked2HypermediaObject)])]
    public class Linked2HypermediaObject : IHypermediaObject
    {
    }

    [HypermediaObject(Classes = [nameof(Linked3HypermediaObject)])]
    public class Linked3HypermediaObject : IHypermediaObject
    {
    }

    public class LinkedHypermediaObjectWithKeyRouteKeyProvider : IKeyProducer
    {
        public object CreateFromHypermediaObject(IHypermediaObject hypermediaObject)
        {
            var ho = (LinkedHypermediaObjectWithKey) hypermediaObject;
            return new {key = ho.Id};
        }

       public object CreateFromKeyObject(object keyObject)
       {
           return new {key = keyObject};
       }
    }

    [HypermediaObject(Classes = [nameof(LinkedHypermediaObjectWithKey)])]
    public class LinkedHypermediaObjectWithKey : IHypermediaObject
    {
        public  int Id { get; set; }
    }

    public class QueryObject : IHypermediaQuery
    {
        public bool ABool { get; set; }
        public int AInt { get; set; }
    }


}
