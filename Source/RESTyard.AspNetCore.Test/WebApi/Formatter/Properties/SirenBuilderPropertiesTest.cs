using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Test.Helpers;

namespace RESTyard.AspNetCore.Test.WebApi.Formatter.Properties
{
    public class SirenBuilderPropertiesTest : SirenBuilderTestBase
    {
        public SirenBuilderPropertiesTest()
        {
            ClassInitBase();
            TestInitBase();
        }

        [Fact]
        public void EmptyObjectTest()
        {
            var routeName = nameof(EmptyHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(EmptyHypermediaObject), routeName, HttpMethods.Get);

            var ho = new EmptyHypermediaObject();
            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(EmptyHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasNoLinks(siren);
        }

        [Fact]
        public void AttributedEmptyObjectTest()
        {
            var routeName = nameof(AttributedEmptyHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(AttributedEmptyHypermediaObject), routeName, HttpMethods.Get);

            var ho = new AttributedEmptyHypermediaObject();
            var siren = SirenConverter.ConvertToJson(ho);

            // class
            var classArray = siren["class"].Should().BeOfType<JsonArray>().Which;
            classArray.Should().HaveCount(2);
            classArray[0]!.ToString().Should().Be("CustomClass1");
            classArray[1]!.ToString().Should().Be("CustomClass2");

            // title
            siren["title"]!.GetValueKind().Should().Be(JsonValueKind.String);
            siren["title"]!.GetValue<string>().Should().Be("A Title");

            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasNoLinks(siren);
        }

        [Fact]
        public void PropertyDuplicateObjectTest()
        {
            var routeName = nameof(PropertyDuplicateHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(PropertyDuplicateHypermediaObject), routeName, HttpMethods.Get);

            var ho = new PropertyDuplicateHypermediaObject();
            var action = () => SirenConverter.ConvertToJson(ho);
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AttributedPropertyTest()
        {
            var routeName = nameof(AttributedPropertyHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(AttributedPropertyHypermediaObject), routeName, HttpMethods.Get);

            var ho = new AttributedPropertyHypermediaObject();
            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(AttributedPropertyHypermediaObject));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasNoLinks(siren);

            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);

            var propertyInfos = typeof(AttributedPropertyHypermediaObject).GetProperties()
                .Where(p =>
                    p.Name != "IgnoredProperty"
                    && p.Name != "Entities"
                    && p.Name != "Links"
                    && p.Name != nameof(IHypermediaObject.HtoTitle))
                .ToList();
            propertiesObject.Should().HaveCount(propertyInfos.Count);

            foreach (var property in propertyInfos)
            {
                string lookupName;
                switch (property.Name)
                {
                    case "Property1":
                        lookupName = "Property1Renamed";
                        break;
                    case "Property2":
                        lookupName = "Property2Renamed";
                        break;
                    default:
                        lookupName = property.Name;
                        break;
                }
                propertiesObject.Should().ContainKey(lookupName).WhoseValue.Should().NotBeNull();
            }
        }

        [Fact]
        public void PropertyDefaultsObjectTest()
        {
            var routeName = nameof(PropertyHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(PropertyHypermediaObject), routeName, HttpMethods.Get);

            var ho = new PropertyHypermediaObject();
            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(PropertyHypermediaObject));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasNoLinks(siren);

            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);

            PropertyHelpers.CompareHypermediaPropertiesAndJson(propertiesObject, ho);
        }

        [Fact]
        public void PropertyDefaultsObjectWriteNoNullPropertiesTest()
        {
            var routeName = nameof(PropertyHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(PropertyHypermediaObject), routeName, HttpMethods.Get);

            var ho = new PropertyHypermediaObject();
            var siren = SirenConverterNoNullProperties.ConvertToJson(ho);

            AssertClassName(siren, nameof(PropertyHypermediaObject));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasNoLinks(siren);

            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);

            PropertyHelpers.CompareHypermediaPropertiesAndJsonNoNullProperties(propertiesObject, ho);
        }

        [Fact]
        public void PropertyObjectTest()
        {
            var routeName = nameof(PropertyHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(PropertyHypermediaObject), routeName, HttpMethods.Get);

            var ho = new PropertyHypermediaObject
            {
                ABool = true,
                AString = "My String",
                AnInt = 1,
                ALong = 2,
                AFloat = 3.1f,
                ADouble = 4.1,

                AnEnum = TestEnum.Value1,
                ANullableEnum = TestEnum.Value1,
                AnEnumWithNames = TestEnumWithNames.Value2,

                ADateTime = new DateTime(2000, 11, 22, 18, 5, 32, 999),
                ADateTimeOffset = new DateTimeOffset(2000, 11, 22, 18, 5, 32, 999, new TimeSpan(0, 2, 0, 0)),
                ATimeSpan = new TimeSpan(1, 2, 3, 4),
                AnUri = new Uri("http://localhost/myuri"),
                ADecimal = 12345,
                ANullableInt = 10,
                DateOnly = new DateOnly(2025, 09, 02),
                TimeOnly = new TimeOnly(12, 01, 35),
                AType = typeof(int)
            };
            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(PropertyHypermediaObject));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasNoLinks(siren);

            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);

            PropertyHelpers.CompareHypermediaPropertiesAndJson(propertiesObject, ho);
        }
    }
}
