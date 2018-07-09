using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using WebApi.HypermediaExtensions.WebApi.Formatter;

namespace WebApi.HypermediaExtensions.Test.WebApi.Formatter.Properties
{
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
            RouteRegister.AddHypermediaObjectRoute(typeof(HypermediaObjectWithListProperties), routeName);

            var ho = new HypermediaObjectWithListProperties();
            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(HypermediaObjectWithListProperties));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            Assert.IsTrue(siren["properties"].Type == JTokenType.Object);
            var propertiesObject = (JObject)siren["properties"];

            PropertieCompareHelpers.CompareHypermediaListPropertiesAndJson(propertiesObject, ho);
        }

        [TestMethod]
        public void SerializeNullListPropertyNoNullPropertiesTest()
        {
            var routeName = typeof(HypermediaObjectWithListProperties).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(HypermediaObjectWithListProperties), routeName);

            var ho = new HypermediaObjectWithListProperties();
            var siren = SirenConverterNoNullProperties.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(HypermediaObjectWithListProperties));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            Assert.IsTrue(siren["properties"].Type == JTokenType.Object);
            var propertiesObject = (JObject)siren["properties"];

            Assert.AreEqual(propertiesObject.Properties().Count(), 0);
        }

        [TestMethod]
        public void SerializeEmptyListProperties()
        {
            var routeName = typeof(HypermediaObjectWithListProperties).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(HypermediaObjectWithListProperties), routeName);

            var ho = new HypermediaObjectWithListProperties();
            ho.AValueList = new List<int>();
            ho.ANullableList = new List<int?>();
            ho.AReferenceList = new List<string>();

            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(HypermediaObjectWithListProperties));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            Assert.IsTrue(siren["properties"].Type == JTokenType.Object);
            var propertiesObject = (JObject)siren["properties"];

            PropertieCompareHelpers.CompareHypermediaListPropertiesAndJson(propertiesObject, ho);
        }

        [TestMethod]
        public void SerializeListProperties()
        {
            var routeName = typeof(HypermediaObjectWithListProperties).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(HypermediaObjectWithListProperties), routeName);

            var ho = new HypermediaObjectWithListProperties();
            ho.AValueList = new List<int> { 3, 5, 7 };
            ho.ANullableList = new List<int?> { 2, null, 4 };
            ho.AReferenceList = new List<string> {"a", "xyz"};
            ho.AValueArray = new[] { 6, 9, 2 };

            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(HypermediaObjectWithListProperties));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            Assert.IsTrue(siren["properties"].Type == JTokenType.Object);
            var propertiesObject = (JObject)siren["properties"];

            PropertieCompareHelpers.CompareHypermediaListPropertiesAndJson(propertiesObject, ho);
        }
    }
    
}
