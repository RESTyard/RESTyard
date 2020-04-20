using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebApi.Hypermedia.Serializer.Tests.ObjectReflection
{
    [TestClass]
    public class When_reflecting_not_a_hto : ObjectReflectionServiceTestBase
    {
        public override void When()
        {
            this.Result = this.ObjectReflectionService.Reflect(typeof(NotAHto));
        }

        [TestMethod]
        public void Then_hto_can_be_reflected()
        {
            Result.Match(ok => Assert.Fail("Can reflect a object which is not a HTO"), error => { });
        }

        private class NotAHto
        {
        }
    }
}