using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;

namespace WebApi.Hypermedia.Serializer.Tests.ObjectReflection.HypermediaObject
{
    [TestClass]
    public class When_reflecting_minimal_hto_with_no_default_self_link : ObjectReflectionServiceTestBase
    {
        public override void When()
        {
            this.Result = this.ObjectReflectionService.Reflect(typeof(NoSelfLinkHto));
        }

        [TestMethod]
        public void Then_result_metainfo_indicates_to_build_no_self_link()
        {
            Result.GetValueOrThrow().HypermediaObjectAttribute.NoDefaultSelfLink.Should().BeTrue();
        }

        [TestMethod]
        public void Then_result_contains_no_link()
        {
            Result.GetValueOrThrow().Links.Should().BeEmpty();
        }

        [HypermediaObject(NoDefaultSelfLink = true)]
        private class NoSelfLinkHto : HypermediaExtensions.Hypermedia.HypermediaObject
        {
        }
    }
}