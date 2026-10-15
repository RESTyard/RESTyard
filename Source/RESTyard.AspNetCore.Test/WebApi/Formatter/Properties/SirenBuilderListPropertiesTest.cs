using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Nodes;

namespace RESTyard.AspNetCore.Test.WebApi.Formatter.Properties
{
    public class SirenBuilderListPropertiesTest : SirenBuilderTestBase
    {
        public SirenBuilderListPropertiesTest()
        {
            ClassInitBase();
            TestInitBase();
        }

        [Fact]
        public void SerializeNullListProperty()
        {
            var routeName = nameof(HypermediaObjectWithListProperties) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(HypermediaObjectWithListProperties), routeName, HttpMethods.Get);

            var ho = new HypermediaObjectWithListProperties();
            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(HypermediaObjectWithListProperties));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasNoLinks(siren);

            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);

            PropertyHelpers.CompareHypermediaListPropertiesAndJson(propertiesObject, ho);
        }

        [Fact]
        public void SerializeNullListPropertyNoNullPropertiesTest()
        {
            var routeName = nameof(HypermediaObjectWithListProperties) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(HypermediaObjectWithListProperties), routeName, HttpMethods.Get);

            var ho = new HypermediaObjectWithListProperties();
            var siren = SirenConverterNoNullProperties.ConvertToJson(ho);

            AssertClassName(siren, nameof(HypermediaObjectWithListProperties));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasNoLinks(siren);

            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);

            propertiesObject.Should().BeEmpty();
        }

        [Fact]
        public void SerializeEmptyListProperties()
        {
            var routeName = nameof(HypermediaObjectWithListProperties) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(HypermediaObjectWithListProperties), routeName, HttpMethods.Get);

            var ho = new HypermediaObjectWithListProperties();
            ho.AValueList = new List<int>();
            ho.ANullableList = new List<int?>();
            ho.AReferenceList = new List<string>();
            ho.ListOfDownCastObjects = new List<object>();

            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(HypermediaObjectWithListProperties));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasNoLinks(siren);

            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);

            PropertyHelpers.CompareHypermediaListPropertiesAndJson(propertiesObject, ho);
        }

        [Fact]
        public void SerializeListProperties()
        {
            var routeName = nameof(HypermediaObjectWithListProperties) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(HypermediaObjectWithListProperties), routeName, HttpMethods.Get);

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
            ho.ListOfDownCastObjects = new List<object> {"Text", 5, new Nested(3)};

            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(HypermediaObjectWithListProperties));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasNoLinks(siren);

            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);

            PropertyHelpers.CompareHypermediaListPropertiesAndJson(propertiesObject, ho);

            AssertObjectList(ho, siren);
            AssertListOfLists(ho, siren);
            AssertListOfDownCastObjects(ho, siren);
        }

        private static void AssertListOfLists(HypermediaObjectWithListProperties ho, JsonObject siren)
        {
            var listOfLists = siren["properties"]!["ListOfLists"]!.AsArray();
            listOfLists.Should().HaveCount(ho.ListOfLists.Count());
            var index = 0;
            foreach (var nested in ho.ListOfLists)
            {
                var nestedList = nested.ToList();
                var innerJArray = listOfLists[index]!.AsArray();
                innerJArray.Should().HaveCount(nestedList.Count);

                var innerIndex = 0;
                foreach (var value in nestedList)
                {
                    innerJArray[innerIndex]!.GetValue<int>().Should().Be(value);
                    innerIndex++;
                }

                index++;
            }
        }

        private static void AssertObjectList(HypermediaObjectWithListProperties ho, JsonObject siren)
        {
            var objectList = siren["properties"]!["AObjectList"]!.AsArray();
            objectList.Should().HaveCount(ho.AObjectList.Count());
            var index = 0;
            foreach (var nested in ho.AObjectList)
            {
                objectList[index]![nameof(Nested.AInt)]!.GetValue<int>().Should().Be(nested.AInt);
                index++;
            }
        }

        private static void AssertListOfDownCastObjects(HypermediaObjectWithListProperties ho, JsonObject siren)
        {
            var downCastList = siren["properties"]!["ListOfDownCastObjects"]!.AsArray();
            downCastList.Should().HaveCount(ho.ListOfDownCastObjects.Count());

            downCastList[0]!.GetValue<string>().Should().Be("Text");
            downCastList[1]!.GetValue<int>().Should().Be(5);
            downCastList[2]![nameof(Nested.AInt)]!.GetValue<int>().Should().Be(3);
        }
    }
    
}
