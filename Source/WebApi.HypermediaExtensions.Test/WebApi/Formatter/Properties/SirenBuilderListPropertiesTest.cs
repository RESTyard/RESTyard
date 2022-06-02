using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace WebApi.HypermediaExtensions.Test.WebApi.Formatter.Properties
{
    using HypermediaExtensions.WebApi.RouteResolver;

    [TestClass]
    public class SirenBuilderListPropertiesTest : SirenBuilderTestBase
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
        public void SerializeNullListProperty()
        {
            var routeName = typeof(HypermediaObjectWithListProperties).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(HypermediaObjectWithListProperties), routeName, HttpMethod.GET);

            var ho = new HypermediaObjectWithListProperties();
            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(HypermediaObjectWithListProperties));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);

            PropertyHelpers.CompareHypermediaListPropertiesAndJson(propertiesObject, ho);
        }

        [TestMethod]
        public void SerializeNullListPropertyNoNullPropertiesTest()
        {
            var routeName = typeof(HypermediaObjectWithListProperties).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(HypermediaObjectWithListProperties), routeName, HttpMethod.GET);

            var ho = new HypermediaObjectWithListProperties();
            var siren = SirenConverterNoNullProperties.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(HypermediaObjectWithListProperties));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);

            Assert.AreEqual(propertiesObject.Properties().Count(), 0);
        }

        [TestMethod]
        public void SerializeEmptyListProperties()
        {
            var routeName = typeof(HypermediaObjectWithListProperties).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(HypermediaObjectWithListProperties), routeName, HttpMethod.GET);

            var ho = new HypermediaObjectWithListProperties();
            ho.AValueList = new List<int>();
            ho.ANullableList = new List<int?>();
            ho.AReferenceList = new List<string>();

            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(HypermediaObjectWithListProperties));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);

            PropertyHelpers.CompareHypermediaListPropertiesAndJson(propertiesObject, ho);
        }

        [TestMethod]
        public void SerializeListProperties()
        {
            var routeName = typeof(HypermediaObjectWithListProperties).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(HypermediaObjectWithListProperties), routeName, HttpMethod.GET);

            var ho = new HypermediaObjectWithListProperties();
            ho.AValueList = new List<int> { 3, 5, 7 };
            ho.ANullableList = new List<int?> { 2, null, 4 };
            ho.AReferenceList = new List<string> {"a", "xyz"};
            ho.AValueArray = new[] { 6, 9, 2, 7 };
            ho.AObjectList = new List<Nested>
            {
                new Nested(3),
                new Nested(5)
            };

            ho.ListOfLists = new List<IEnumerable<int>>
            {
                new List<int> { 3,4,5},
                new List<int> { 6,7,8}
            };

            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(HypermediaObjectWithListProperties));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);

            PropertyHelpers.CompareHypermediaListPropertiesAndJson(propertiesObject, ho);

            AssertObjectList(ho, siren);
            AssertListOfLists(ho, siren);
        }

        private static void AssertListOfLists(HypermediaObjectWithListProperties ho, JObject siren)
        {
            Assert.AreEqual(ho.ListOfLists.Count(), siren["properties"]["ListOfLists"].Count());
            var index = 0;
            foreach (var nested in ho.ListOfLists)
            {
                var nestedList = nested.ToList();
                var innerJArray = siren["properties"]["ListOfLists"][index].Value<JArray>();
                Assert.AreEqual(nestedList.Count(), innerJArray.Count);

                var innerIndex = 0;
                foreach (var value in nestedList)
                {
                    Assert.AreEqual(value, innerJArray[innerIndex].Value<int>());
                    innerIndex++;
                }

                index++;
            }
        }

        private static void AssertObjectList(HypermediaObjectWithListProperties ho, JObject siren)
        {
            Assert.AreEqual(ho.AObjectList.Count(), siren["properties"]["AObjectList"].Count());
            var index = 0;
            foreach (var nested in ho.AObjectList)
            {
                Assert.AreEqual(nested.AInt, siren["properties"]["AObjectList"][index].Value<JObject>()[nameof(Nested.AInt)].Value<int>());
                index++;
            }
        }
    }
    
}
