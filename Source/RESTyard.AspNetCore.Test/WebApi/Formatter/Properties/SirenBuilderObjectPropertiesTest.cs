using Microsoft.AspNetCore.Http;

namespace RESTyard.AspNetCore.Test.WebApi.Formatter.Properties
{
    public class SirenBuilderObjectPropertiesTest : SirenBuilderTestBase
    {
        public SirenBuilderObjectPropertiesTest()
        {
            ClassInitBase();
            TestInitBase();
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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
