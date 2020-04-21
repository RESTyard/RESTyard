using Bluehands.Hypermedia.Relations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;

namespace WebApi.Hypermedia.Serializer.Tests.ObjectReflection.HypermediaObject
{
    [TestClass]
    public class When_reflecting_minimal_hto_with_duplicate_manual_self_link : ObjectReflectionServiceTestBase
    {
        public override void When()
        {
            this.Result = this.ObjectReflectionService.Reflect(typeof(TestHto));
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
        private class TestHto : HypermediaExtensions.Hypermedia.HypermediaObject
        {
            [Link(DefaultHypermediaRelations.Self)]
            public TestHto ManualSelfLink { get; private set; }
        }
    }
}