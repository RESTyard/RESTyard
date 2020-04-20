using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;

namespace WebApi.Hypermedia.Serializer.Tests.ObjectReflection.HypermediaObject
{
    [TestClass]
    public class When_reflecting_minimal_hto_with_meta_info : ObjectReflectionServiceTestBase
    {
        public override void When()
        {
            this.Result = this.ObjectReflectionService.Reflect(typeof(MinimalWithMetaInfoHto));
        }

        [TestMethod]
        public void Then_result_meta_info_contains_class()
        {
            Result.GetValueOrThrow().HypermediaObjectAttribute.Classes.Length.Should().Be(2);
        }

        [TestMethod]
        public void Then_result_meta_info_contains_class_1()
        {
            Result.GetValueOrThrow().HypermediaObjectAttribute.Classes[0].Should().Be("test.minimal");
        }

        [TestMethod]
        public void Then_result_meta_info_contains_class_2()
        {
            Result.GetValueOrThrow().HypermediaObjectAttribute.Classes[1].Should().Be("test.minimal.WithMetaInfo");
        }

        [TestMethod]
        public void Then_result_meta_info_contains_title()
        {
            Result.GetValueOrThrow().HypermediaObjectAttribute.Title.Should().Be("A small title");
        }

        [HypermediaObject(Title = "A small title", Classes = new[] { "test.minimal", "test.minimal.WithMetaInfo" })]
        private class MinimalWithMetaInfoHto : HypermediaExtensions.Hypermedia.HypermediaObject
        {
        }
    }
}