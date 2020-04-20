using System.Linq;
using Bluehands.Hypermedia.Relations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;

namespace WebApi.Hypermedia.Serializer.Tests.ObjectReflection.HypermediaObject
{
    [TestClass]
    public class When_reflecting_minimal_hto : ObjectReflectionServiceTestBase
    {
        public override void When()
        {
            this.Result = this.ObjectReflectionService.Reflect(typeof(MinimalHto));
        }

        [TestMethod]
        public void Then_hto_can_be_reflected()
        {
            Result.GetValueOrThrow();
        }

        [TestMethod]
        public void Then_result_contains_hto_type()
        {
            Result.GetValueOrThrow().HypermediaObjectType.Should().Be(typeof(MinimalHto));
        }

        [TestMethod]
        public void Then_result_contains_no_actions()
        {
            Result.GetValueOrThrow().Actions.Should().BeEmpty();
        }

        [TestMethod]
        public void Then_result_contains_no_entities()
        {
            Result.GetValueOrThrow().Entities.Should().BeEmpty();
        }

        [TestMethod]
        public void Then_result_contains_only_one_link()
        {
            Result.GetValueOrThrow().Links.Count.Should().Be(1);
        }

        [TestMethod]
        public void Then_result_self_link_has_relation_self()
        {
            var linkAttribute = Result.GetValueOrThrow().Links.First().PrimaryHypermediaAttribute.GetValueOrThrow().As<Link>();
            linkAttribute.Relations.Single().Should().Be(DefaultHypermediaRelations.Self);
        }

        [TestMethod]
        public void Then_result_meta_info_contains_no_classes()
        {
            Result.GetValueOrThrow().HypermediaObjectAttribute.Classes.Should().BeEmpty();
        }

        [TestMethod]
        public void Then_result_metainfo_contains_no_title()
        {
            Result.GetValueOrThrow().HypermediaObjectAttribute.Title.Should().BeEmpty();
        }

        [TestMethod]
        public void Then_result_metainfo_indicates_to_build_self_link()
        {
            Result.GetValueOrThrow().HypermediaObjectAttribute.NoDefaultSelfLink.Should().BeFalse();
        }

        [HypermediaObject]
        private class MinimalHto : HypermediaExtensions.Hypermedia.HypermediaObject
        {
        }
    }
}