using System.Linq;
using Bluehands.Hypermedia.Relations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.WebApi.Serializer;

namespace WebApi.Hypermedia.ModelFactory.Test.ObjectReflection.Links
{
    [TestClass]
    public class When_building_model_for_hto_with_non_hto_object_link : ModelFactoryTestBase
    {
        public override void When()
        {
            this.Result = ModelFactory2.Build(typeof(TestHto), new ModelBuilderOptions());
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
        public void Then_result_link_has_self_relation()
        {
            Result.GetValueOrThrow().Links.First().Relations.Contains(DefaultHypermediaRelations.Self);
        }

        [TestMethod]
        public void Then_result_link_has_one_relation()
        {
            Result.GetValueOrThrow().Links.Length.Should().Be(1);
        }

        [HypermediaObject(NoDefaultSelfLink = true)]
        private class TestHto
        {
            [Link("MyRelation")]
            public Referenced NonHtoLink { get; private set; }

        }

        private class Referenced
        {
        }
    }
}