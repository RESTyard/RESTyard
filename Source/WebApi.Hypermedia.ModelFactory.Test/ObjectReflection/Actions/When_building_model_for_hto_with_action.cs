using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.WebApi.Serializer;

namespace WebApi.Hypermedia.ModelFactory.Test.ObjectReflection.Actions
{
    [TestClass]
    public class When_building_model_for_hto_with_action : ModelFactoryTestBase
    {
        public override void When()
        {
            this.Result = ModelFactory2.Build(typeof(TestHto));
        }

        [TestMethod]
        public void Then_hto_can_be_reflected()
        {
            Result.GetValueOrThrow();
        }

        [HypermediaObject]
        private class TestHto
        {
            [Action(Title = "This is a Action with no return", Name = "MyVoidAction")]
            public HypermediaAction2 VoidAction { get; set; } = HypermediaAction2.Available();
        }
    }
}