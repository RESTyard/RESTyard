using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Nodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RESTyard.AspNetCore.Test.WebApi.Formatter.Properties
{
    [TestClass]
    public class SirenBuilderObjectPropertiesTest : SirenBuilderTestBase
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
        public void PropertyNestedClass()
        {
            var routeName = nameof(PropertyNestedClassHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(PropertyNestedClassHypermediaObject), routeName, HttpMethods.Get);

            var ho = new PropertyNestedClassHypermediaObject
            {
                AChild = new AttributedPropertyHypermediaObject
                {
                    Property1 = true,
                    Property2 = true,
                    NotRenamed = true,
                    IgnoredProperty = true
                }
            };

            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(PropertyNestedClassHypermediaObject));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasNoLinks(siren);

            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);

            Assert.AreEqual(1, propertiesObject.Count);
            var nestedJObject = siren["properties"]![nameof(PropertyNestedClassHypermediaObject.AChild)]!.AsObject();

            // one property is ignored
            Assert.AreEqual(3, nestedJObject.Count);
            Assert.AreEqual(ho.AChild.Property1, nestedJObject["Property1Renamed"]!.GetValue<bool>());
            Assert.AreEqual(ho.AChild.Property2, nestedJObject["Property2Renamed"]!.GetValue<bool>());
            Assert.AreEqual(ho.AChild.NotRenamed, nestedJObject[nameof(AttributedPropertyHypermediaObject.NotRenamed)]!.GetValue<bool>());
        }

        [TestMethod]
        public void PropertyNestedClassNull()
        {
            var routeName = nameof(PropertyNestedClassHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(PropertyNestedClassHypermediaObject), routeName, HttpMethods.Get);

            var ho = new PropertyNestedClassHypermediaObject
            {
                AChild = null
            };

            var siren = SirenConverter.ConvertToJson(ho);
            
            AssertClassName(siren, nameof(PropertyNestedClassHypermediaObject));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasNoLinks(siren);

            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);

            Assert.AreEqual(1, propertiesObject.Count);
            Assert.IsNull(propertiesObject.First().Value);
        }

        [TestMethod]
        public void PropertyNestedClassNullNoNullProperties()
        {
            var routeName = nameof(PropertyNestedClassHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(PropertyNestedClassHypermediaObject), routeName, HttpMethods.Get);

            var ho = new PropertyNestedClassHypermediaObject
            {
                AChild = null
            };

            var siren = SirenConverterNoNullProperties.ConvertToJson(ho);

            AssertClassName(siren, nameof(PropertyNestedClassHypermediaObject));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasNoLinks(siren);

            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);

            Assert.AreEqual(propertiesObject.Count, 0);
        }
    }
    
}
