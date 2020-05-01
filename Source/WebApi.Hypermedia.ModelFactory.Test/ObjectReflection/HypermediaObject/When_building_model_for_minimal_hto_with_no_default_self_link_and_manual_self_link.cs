using Bluehands.Hypermedia.Relations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.WebApi.Serializer;

namespace WebApi.Hypermedia.ModelFactory.Test.ObjectReflection.HypermediaObject
{
    [TestClass]
    public class When_building_model_for_minimal_hto_with_duplicate_manual_self_link : ModelFactoryTestBase
    {
        public override void When()
        {
            this.Result = ModelFactory2.Build(typeof(TestHto));
        }

        [TestMethod]
        public void Then_result_contains_error()
        {
            Result.Match(ok => Assert.Fail("Must be an error, duplicate self link"), error =>
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    Assert.Fail("Must contain error message");
                }
            });
        }

        [HypermediaObject]
        private class TestHto
        {
            [Link(DefaultHypermediaRelations.Self)]
            public TestHto ManualSelfLink { get; private set; }
        }
    }
}