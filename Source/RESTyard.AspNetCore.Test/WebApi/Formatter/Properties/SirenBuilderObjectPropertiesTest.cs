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

            propertiesObject.Should().HaveCount(1);
            var nestedJObject = siren["properties"]![nameof(PropertyNestedClassHypermediaObject.AChild)]!.AsObject();

            // one property is ignored
            nestedJObject.Should().HaveCount(3);
            nestedJObject.Should().ContainKey("Property1Renamed").WhoseValue!.GetValue<bool>().Should().Be(ho.AChild.Property1);
            nestedJObject.Should().ContainKey("Property2Renamed").WhoseValue!.GetValue<bool>().Should().Be(ho.AChild.Property2);
            nestedJObject.Should().ContainKey(nameof(AttributedPropertyHypermediaObject.NotRenamed)).WhoseValue!.GetValue<bool>().Should().Be(ho.AChild.NotRenamed);
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

            propertiesObject.Should().ContainSingle().Which.Value.Should().BeNull();
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

            propertiesObject.Should().BeEmpty();
        }
    }
    
}
