using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.WebApi.Serializer;

namespace WebApi.Hypermedia.ModelFactory.Test.ObjectReflection.HypermediaObject
{
    [TestClass]
    public class When_building_model_option_default_selflink_off : ModelFactoryTestBase
    {
        public override void When()
        {
            this.Result = ModelFactory2.Build(typeof(MinimalHto), new ModelBuilderOptions() {CreateDefaultSelfLink = false} );
        }

        [TestMethod]
        public void Then_hto_can_be_reflected()
        {
            Result.GetValueOrThrow();
        }

        [TestMethod]
        public void Then_result_contains_one_link()
        {
            Result.GetValueOrThrow().Links.Length.Should().Be(0);
        }

        [HypermediaObject]
        private class MinimalHto
        {
        }
    }
}