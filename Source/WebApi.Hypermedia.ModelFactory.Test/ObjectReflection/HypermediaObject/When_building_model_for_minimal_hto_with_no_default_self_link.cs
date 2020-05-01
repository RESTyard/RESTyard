using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.WebApi.Serializer;

namespace WebApi.Hypermedia.ModelFactory.Test.ObjectReflection.HypermediaObject
{
    [TestClass]
    public class When_building_model_for_minimal_hto_with_no_default_self_link : ModelFactoryTestBase
    {
        public override void When()
        {
            this.Result = ModelFactory2.Build(typeof(NoSelfLinkHto));
        }

        [TestMethod]
        public void Then_result_metainfo_indicates_to_build_no_self_link()
        {
            Assert.Inconclusive("Do we need this in the model?");
            //Result.GetValueOrThrow().HypermediaObjectAttribute.NoDefaultSelfLink.Should().BeTrue();
        }

        [TestMethod]
        public void Then_result_contains_no_link()
        {
            Result.GetValueOrThrow().Links.Should().BeEmpty();
        }

        [HypermediaObject(NoDefaultSelfLink = true)]
        private class NoSelfLinkHto
        {
        }
    }
}