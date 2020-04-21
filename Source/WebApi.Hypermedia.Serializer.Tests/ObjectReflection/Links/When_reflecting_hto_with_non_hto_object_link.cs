using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;

namespace WebApi.Hypermedia.Serializer.Tests.ObjectReflection.Links
{
    [TestClass]
    public class When_reflecting_hto_with_non_hto_object_link : ObjectReflectionServiceTestBase
    {
        public override void When()
        {
            this.Result = this.ObjectReflectionService.Reflect(typeof(TestHto));
        }

        [TestMethod]
        public void Then_hto_can_be_reflected()
        {
            Result.GetValueOrThrow();
        }

        [TestMethod]
        public void Then_result_contains_error()
        {
            Result.Match(ok => Assert.Fail("Must be an error, only hto or URI can be a link"), error =>
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    Assert.Fail("Must contain error message");
                }
            });
        }

        [TestMethod]
        public void Then_result_contains_no_link()
        {
            Result.GetValueOrThrow().Links.Should().BeEmpty();
        }

        [TestMethod]
        public void Then_result_self_link_has_relation()
        {
            var linkAttribute = Result.GetValueOrThrow().Links.First().PrimaryHypermediaAttribute.GetValueOrThrow().As<Link>();
            linkAttribute.Relations.Single().Should().Be("MyRelation");
        }

        [HypermediaObject(NoDefaultSelfLink = true)]
        private class TestHto : HypermediaExtensions.Hypermedia.HypermediaObject
        {
            [Link("MyRelation")]
            public Referenced NonHtoLink { get; private set; }

        }

        private class Referenced
        {
        }
    }
}