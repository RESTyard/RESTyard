using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi.HypermediaExtensions.WebApi.Serializer;

namespace WebApi.Hypermedia.ModelFactory.Test.ObjectReflection.HypermediaObject
{
    [TestClass]
    public class When_building_model_for__not_a_hto : ModelFactoryTestBase
    {
        public override void When()
        {
            this.Result = ModelFactory2.Build(typeof(NotAHto));
        }

        [TestMethod]
        public void Then_hto_can_be_reflected()
        {
            Result.Match(ok => Assert.Fail("Can reflect a object which is not a HTO"), error => { });
        }

        private class NotAHto
        {
        }
    }
}