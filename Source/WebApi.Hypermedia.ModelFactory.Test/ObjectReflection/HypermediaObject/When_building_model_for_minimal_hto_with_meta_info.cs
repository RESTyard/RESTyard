using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.WebApi.Serializer;

namespace WebApi.Hypermedia.ModelFactory.Test.ObjectReflection.HypermediaObject
{
    [TestClass]
    public class When_building_model_for_minimal_hto_with_meta_info : ModelFactoryTestBase
    {
        public override void When()
        {
            this.Result = ModelFactory2.Build(typeof(MinimalWithMetaInfoHto), new ModelBuilderOptions());
        }

        [TestMethod]
        public void Then_result_meta_info_contains_class()
        {
            Result.GetValueOrThrow().Classes.Length.Should().Be(2);
        }

        [TestMethod]
        public void Then_result_meta_info_contains_class_1()
        {
            Result.GetValueOrThrow().Classes[0].Should().Be("test.minimal");
        }

        [TestMethod]
        public void Then_result_meta_info_contains_class_2()
        {
            Result.GetValueOrThrow().Classes[1].Should().Be("test.minimal.WithMetaInfo");
        }

        [TestMethod]
        public void Then_result_meta_info_contains_title()
        {
            Result.GetValueOrThrow().Title.Should().Be("A small title");
        }

        [HypermediaObject(Title = "A small title", Classes = new[] { "test.minimal", "test.minimal.WithMetaInfo" })]
        private class MinimalWithMetaInfoHto
        {
        }
    }
}