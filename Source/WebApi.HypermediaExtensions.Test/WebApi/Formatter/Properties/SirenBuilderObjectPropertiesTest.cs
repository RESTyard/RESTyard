using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using RESTyard.AspNetCore.WebApi.RouteResolver;

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
            var routeName = typeof(PropertyNestedClassHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(PropertyNestedClassHypermediaObject), routeName, HttpMethod.GET);

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

            AssertDefaultClassName(siren, typeof(PropertyNestedClassHypermediaObject));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);

            Assert.AreEqual(1, propertiesObject.Properties().Count());
            var nestedJObject = (JObject)siren["properties"][nameof(PropertyNestedClassHypermediaObject.AChild)];

            // one property is ignored
            Assert.AreEqual(3, nestedJObject.Properties().Count());
            Assert.AreEqual(ho.AChild.Property1, nestedJObject["Property1Renamed"].Value<bool>());
            Assert.AreEqual(ho.AChild.Property2, nestedJObject["Property2Renamed"].Value<bool>());
            Assert.AreEqual(ho.AChild.NotRenamed, nestedJObject[nameof(AttributedPropertyHypermediaObject.NotRenamed)].Value<bool>());
        }

        [TestMethod]
        public void PropertyNestedClassNull()
        {
            var routeName = typeof(PropertyNestedClassHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(PropertyNestedClassHypermediaObject), routeName, HttpMethod.GET);

            var ho = new PropertyNestedClassHypermediaObject
            {
                AChild = null
            };

            var siren = SirenConverter.ConvertToJson(ho);
            
            AssertDefaultClassName(siren, typeof(PropertyNestedClassHypermediaObject));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);

            Assert.AreEqual(1, propertiesObject.Properties().Count());
            Assert.AreEqual(JValue.CreateNull(), propertiesObject.Properties().First().Value);
        }

        [TestMethod]
        public void PropertyNestedClassNullNoNullProperties()
        {
            var routeName = typeof(PropertyNestedClassHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(PropertyNestedClassHypermediaObject), routeName, HttpMethod.GET);

            var ho = new PropertyNestedClassHypermediaObject
            {
                AChild = null
            };

            var siren = SirenConverterNoNullProperties.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(PropertyNestedClassHypermediaObject));
            AssertEmptyEntities(siren);
            AssertEmptyActions(siren);
            AssertHasOnlySelfLink(siren, routeName);

            var propertiesObject = PropertyHelpers.GetPropertiesJObject(siren);

            Assert.AreEqual(propertiesObject.Properties().Count(), 0);
        }
    }
    
}
