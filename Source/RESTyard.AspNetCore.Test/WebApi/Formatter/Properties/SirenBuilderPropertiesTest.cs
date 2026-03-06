using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using RESTyard.AspNetCore.Test.Helpers;

namespace RESTyard.AspNetCore.Test.WebApi.Formatter.Properties
{
    [TestClass]
    public class SirenBuilderPropertiesTest : SirenBuilderTestBase
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

        [TestMethod]
        public void AttributedEmptyObjectTest()
        {
            var routeName = nameof(AttributedEmptyHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(AttributedEmptyHypermediaObject), routeName, HttpMethods.Get);

            var ho = new AttributedEmptyHypermediaObject();
            var siren = SirenConverter.ConvertToJson(ho);

            // class
            Assert.IsTrue(siren["class"].Type == JTokenType.Array);
            var classArray = (JArray)siren["class"];
            Assert.AreEqual(classArray.Count, 2);
            Assert.IsTrue(siren["class"][0].ToString() == "CustomClass1");
            Assert.IsTrue(siren["class"][1].ToString() == "CustomClass2");

            // title
            Assert.IsTrue(siren["title"].Type == JTokenType.String);
            Assert.AreEqual(siren["title"], "A Title");

            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasNoLinks(siren);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PropertyDuplicateObjectTest()
        {
            var routeName = nameof(PropertyDuplicateHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(PropertyDuplicateHypermediaObject), routeName, HttpMethods.Get);

            var ho = new PropertyDuplicateHypermediaObject();
            SirenConverter.ConvertToJson(ho);
        }

        [TestMethod]
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
                    && p.Name != "Links")
                .ToList();
            Assert.AreEqual(propertiesObject.Properties().Count(), propertyInfos.Count);

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
                Assert.IsTrue(propertiesObject[lookupName] != null);
            }
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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
