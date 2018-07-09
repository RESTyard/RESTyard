using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

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
        public void SerializeListProperty()
        {
            //var routeName = typeof(PropertyHypermediaObject).Name + "_Route";
            //RouteRegister.AddHypermediaObjectRoute(typeof(PropertyHypermediaObject), routeName);

            //var ho = new PropertyHypermediaObject();
            //var siren = SirenConverter.ConvertToJson(ho);

            //AssertDefaultClassName(siren, typeof(PropertyHypermediaObject));
            //AssertEmptyEntities(siren);
            //AssertEmptyActions(siren);
            //AssertHasOnlySelfLink(siren, routeName);

            //Assert.IsTrue(siren["properties"].Type == JTokenType.Object);
            //var propertiesObject = (JObject)siren["properties"];

            //PropertieCompareHelpers.CompareHypermediaPropertiesAndJson(propertiesObject, ho);
        }
    }
    
}
