using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebApi.Hypermedia.ModelFactory.Test.ObjectReflection.Links
{
    [TestClass]
    public class When_building_model_for_hto_with_object_reference_link : ModelFactoryTestBase
    {
        public override void When()
        {
         //   this.Result = ModelFactory2.Build(typeof(TestHto));
        }

        [TestMethod]
        public void TODO()
        {
            Assert.Fail("HypermediaObjectReference still needs a type derived form HypermediaObject");
        }

        //[TestMethod]
        //public void Then_hto_can_be_reflected()
        //{
        //    Result.GetValueOrThrow();
        //}

        //[TestMethod]
        //public void Then_result_contains_one_link()
        //{
        //    Result.GetValueOrThrow().Links.Length.Should().Be(1);
        //}

        //[TestMethod]
        //public void Then_result_link_has_relation()
        //{
        //    Result.GetValueOrThrow().Links.First().Relations.Contains("MyRelation");
        //}

        //[TestMethod]
        //public void Then_result_link_has_one_relation()
        //{
        //    Result.GetValueOrThrow().Links.Length.Should().Be(1);
        //}

        //[HypermediaObject(NoDefaultSelfLink = true)]
        //private class TestHto
        //{
        //    [Link("MyRelation")]
        //    public HypermediaObjectReference<ReferencedHto> HtoLink { get; private set; }

        //}

        //[HypermediaObject(NoDefaultSelfLink = true)]
        //private class ReferencedHto
        //{
        //}
    }
}