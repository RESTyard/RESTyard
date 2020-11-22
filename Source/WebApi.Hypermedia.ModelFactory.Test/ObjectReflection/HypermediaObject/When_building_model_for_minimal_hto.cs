using System.Linq;
using Bluehands.Hypermedia.Model;
using Bluehands.Hypermedia.Relations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.WebApi.Serializer;

namespace WebApi.Hypermedia.ModelFactory.Test.ObjectReflection.HypermediaObject
{
    [TestClass]
    public class When_building_model_for_minimal_hto : ModelFactoryTestBase
    {
        public override void When()
        {
            this.Result = ModelFactory2.Build(typeof(MinimalHto), new ModelBuilderOptions());
        }

        [TestMethod]
        public void Then_hto_can_be_reflected()
        {
            Result.GetValueOrThrow();
        }

        [TestMethod]
        public void Then_result_contains_hto_type()
        {
            var expected = new EntityKey(typeof(MinimalHto).Name, typeof(MinimalHto).Namespace);
            Result.GetValueOrThrow().Key.Should().Equals(expected);
        }

        [TestMethod]
        [Ignore("Actions are not implemented")]
        public void Then_result_contains_no_actions()
        {
            //Result.GetValueOrThrow().Actions.Should().BeEmpty();
        }

        [TestMethod]
        public void Then_result_contains_no_entities()
        {
            Result.GetValueOrThrow().Entities.Should().BeEmpty();
        }

        [TestMethod]
        public void Then_result_contains_one_link()
        {
            Result.GetValueOrThrow().Links.Length.Should().Be(1);
        }

        [TestMethod]
        public void Then_result_self_link_has_relation_self()
        {
            var linkAttribute = Result.GetValueOrThrow().Links.First().Relations.Contains(DefaultHypermediaRelations.Self);
        }

        [TestMethod]
        public void Then_result_self_link_has_only_one_relation()
        {
            var linkAttribute = Result.GetValueOrThrow().Links.First().Relations.Length.Should().Be(1);
        }

        [TestMethod]
        public void Then_result_meta_info_contains_no_classes()
        {
            Result.GetValueOrThrow().Classes.Should().BeEmpty();
        }

        [TestMethod]
        public void Then_result_metainfo_contains_no_title()
        {
            Result.GetValueOrThrow().Title.Should().BeEmpty();
        }

        [HypermediaObject]
        private class MinimalHto
        {
        }
    }
}