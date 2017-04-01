using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Attributes;
using WebApiHypermediaExtensionsCore.Test.Helpers;
using WebApiHypermediaExtensionsCore.Util.Enum;
using WebApiHypermediaExtensionsCore.WebApi.Formatter;

namespace WebApiHypermediaExtensionsCore.Test.WebApi.Formatter
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
            var routeName = typeof(EmptyHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(EmptyHypermediaObject), routeName);

            var ho = new EmptyHypermediaObject();
            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(EmptyHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);
        }

        [TestMethod]
        public void AttributedEmptyObjectTest()
        {
            var routeName = typeof(AttributedEmptyHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(AttributedEmptyHypermediaObject), routeName);

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
            AssertHasOnlySelfLink(siren, routeName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PropertyDuplicateObjectTest()
        {
            var routeName = typeof(PropertyDuplicateHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(PropertyDuplicateHypermediaObject), routeName);

            var ho = new PropertyDuplicateHypermediaObject();
            SirenConverter.ConvertToJson(ho);
        }

        [TestMethod]
        public void AttributedPropertyTest()
        {
            var routeName = typeof(AttributedPropertyHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(AttributedPropertyHypermediaObject), routeName);

            var ho = new AttributedPropertyHypermediaObject();
            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(AttributedPropertyHypermediaObject));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            Assert.IsTrue(siren["properties"].Type == JTokenType.Object);
            var propertiesObject = (JObject)siren["properties"];

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
            var routeName = typeof(PropertyHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(PropertyHypermediaObject), routeName);

            var ho = new PropertyHypermediaObject();
            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(PropertyHypermediaObject));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            Assert.IsTrue(siren["properties"].Type == JTokenType.Object);
            var propertiesObject = (JObject)siren["properties"];

            CompareHypermediaAndJson(propertiesObject, ho);
        }

        [TestMethod]
        public void PropertyDefaultsObjectWriteNoNullPropertiesTest()
        {
            var routeName = typeof(PropertyHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(PropertyHypermediaObject), routeName);

            var ho = new PropertyHypermediaObject();
            var sirenBuilderWithNoNullProperties = CreateSirenConverter(new SirenConverterConfiguration {WriteNullProperties = false});
            var siren = sirenBuilderWithNoNullProperties.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(PropertyHypermediaObject));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            Assert.IsTrue(siren["properties"].Type == JTokenType.Object);
            var propertiesObject = (JObject)siren["properties"];

            CompareHypermediaAndJsonNoNullProperties(propertiesObject, ho);
        }

        [TestMethod]
        public void PropertyObjectTest()
        {
            var routeName = typeof(PropertyHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(PropertyHypermediaObject), routeName);

            var ho = new PropertyHypermediaObject
            {
                ABool = true,
                AString = "My String",
                AInt = 1,
                ALong = 2,
                AFloat = 3.1f,
                ADouble = 4.1,

                AEnum = TestEnum.Value1,
                AEnumWithNames = TestEnumWithNames.Value2,

                ADateTime = new DateTime(2000, 11, 22, 18, 5, 32, 999),
                ADateTimeOffset = new DateTimeOffset(2000, 11, 22, 18, 5, 32, 999, new TimeSpan(0, 2, 0, 0)),
                ATimeSpan = new TimeSpan(1, 2, 3, 4),
                ADecimal = 12345,
                ANullableInt = 10,
            };
            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(PropertyHypermediaObject));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            Assert.IsTrue(siren["properties"].Type == JTokenType.Object);
            var propertiesObject = (JObject)siren["properties"];

            CompareHypermediaAndJson(propertiesObject, ho);
        }

        [TestMethod]
        public void PropertyNestedClassHypermediaObject()
        {
            var routeName = typeof(PropertyNestedClassHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(PropertyNestedClassHypermediaObject), routeName);

            var ho = new PropertyNestedClassHypermediaObject()
            {
                AChild = new ChildClass()
            };

            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(PropertyNestedClassHypermediaObject));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            Assert.IsTrue(siren["properties"].Type == JTokenType.Object);
            var propertiesObject = (JObject)siren["properties"];

            Assert.AreEqual(propertiesObject.Properties().Count(), 0);
        }

        [TestMethod]
        public void PropertyNestedClassNullHypermediaObject()
        {
            var routeName = typeof(PropertyNestedClassHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(PropertyNestedClassHypermediaObject), routeName);

            var ho = new PropertyNestedClassHypermediaObject()
            {
                AChild = null
            };

            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(PropertyNestedClassHypermediaObject));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            Assert.IsTrue(siren["properties"].Type == JTokenType.Object);
            var propertiesObject = (JObject)siren["properties"];

            Assert.AreEqual(propertiesObject.Properties().Count(), 0);
        }

        private static void CompareHypermediaAndJson(JObject propertiesObject, PropertyHypermediaObject ho)
        {
            var propertyInfos = typeof(PropertyHypermediaObject).GetProperties()
                .Where(p => p.Name != "Entities" && p.Name != "Links")
                .ToList();
            Assert.AreEqual(propertiesObject.Properties().Count(), propertyInfos.Count);

            CompareNotNullProperties(propertiesObject, ho);

            Assert.IsFalse(propertiesObject["AString"].HasValues);
            Assert.IsFalse(propertiesObject["ANullableInt"].HasValues);
        }

        private static void CompareNotNullProperties(JObject propertiesObject, PropertyHypermediaObject ho)
        {
            Assert.AreEqual(propertiesObject["ABool"].ToString(), ho.ABool.ToString());

            Assert.AreEqual(propertiesObject["AInt"].ToString(), ho.AInt.ToInvariantString());
            Assert.AreEqual(propertiesObject["ALong"].ToString(), ho.ALong.ToInvariantString());
            Assert.AreEqual(((float) propertiesObject["AFloat"]).ToInvariantString(), ho.AFloat.ToInvariantString());
            Assert.AreEqual(((double) propertiesObject["ADouble"]).ToInvariantString(), ho.ADouble.ToInvariantString());

            Assert.AreEqual(propertiesObject["AEnum"].ToString(), EnumHelper.GetEnumMemberValue(ho.AEnum));
            Assert.AreEqual(propertiesObject["AEnumWithNames"].ToString(), EnumHelper.GetEnumMemberValue(ho.AEnumWithNames));

            Assert.AreEqual(((IFormattable) propertiesObject["ADateTime"]).ToStringZNotation(),
                ho.ADateTime.ToStringZNotation());
            Assert.AreEqual(((IFormattable) propertiesObject["ADateTimeOffset"]).ToStringZNotation(),
                ho.ADateTimeOffset.ToStringZNotation());
            Assert.AreEqual(propertiesObject["ATimeSpan"].ToString(), ho.ATimeSpan.ToInvariantString());
            Assert.AreEqual(propertiesObject["ADecimal"].ToString(), ho.ADecimal.ToInvariantString());
        }

        private static void CompareHypermediaAndJsonNoNullProperties(JObject propertiesObject, PropertyHypermediaObject ho)
        {
            var propertyInfos = typeof(PropertyHypermediaObject).GetProperties()
                .Where(p => p.Name != "Entities" && p.Name != "Links")
                .ToList();
            Assert.AreEqual(propertiesObject.Properties().Count(), propertyInfos.Count - 2);

            CompareNotNullProperties(propertiesObject, ho);

            Assert.IsNull(propertiesObject["AString"]);
            Assert.IsNull(propertiesObject["ANullableInt"]);
        }


    }

    public class EmptyHypermediaObject : HypermediaObject
    {
    }

    [HypermediaObject(Title = "A Title", Classes = new[] { "CustomClass1", "CustomClass2" })]
    public class AttributedEmptyHypermediaObject : HypermediaObject
    {
    }

    public class PropertyDuplicateHypermediaObject : HypermediaObject
    {
        [HypermediaProperty(Name = "DuplicateRename")]
        public bool Property1 { get; set; }

        [HypermediaProperty(Name = "DuplicateRename")]
        public bool Property2 { get; set; }
    }

    public class PropertyNestedClassHypermediaObject : HypermediaObject
    {
        public ChildClass AChild { get; set; }
    }

    public class AttributedPropertyHypermediaObject : HypermediaObject
    {
        [HypermediaProperty(Name = "Property1Renamed")]
        public bool Property1 { get; set; }

        [HypermediaProperty(Name = "Property2Renamed")]
        public bool Property2 { get; set; }

        [FormatterIgnoreHypermediaProperty]
        public bool IgnoredProperty { get; set; }

        public bool NotRenamed { get; set; }
    }

    public class PropertyHypermediaObject : HypermediaObject
    {
        public bool ABool { get; set; }
        public string AString { get; set; }
        public int AInt { get; set; }
        public long ALong { get; set; }
        public float AFloat { get; set; }
        public double ADouble { get; set; }

        public TestEnum AEnum { get; set; }
        public TestEnumWithNames AEnumWithNames { get; set; }

        public DateTime ADateTime { get; set; }
        public DateTimeOffset ADateTimeOffset { get; set; }
        public TimeSpan ATimeSpan { get; set; }
        public decimal ADecimal { get; set; }
        public int? ANullableInt { get; set; }
    }

    public class ChildClass
    {
        public bool ABool { get; set; }
    }
}
