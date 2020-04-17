using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.WebApi.Serializer.Reflection;
using FluentAssertions;

namespace WebApi.Hypermedia.Serializer.Tests
{
    [TestClass]
    public class ObjectReflectionServiceTest
    {

        public ObjectReflectionService ObjectReflectionService { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            ObjectReflectionService = new ObjectReflectionService();
        }

        [TestMethod]
        public void TestMethod1()
        {
            ObjectReflectionService.Reflect(typeof(TestHypermediaObject)).Should().Match(e => e.As<FunicularSwitch.Result<ObjectReflection>>().IsOk);
        }
    }
    
    class TestHypermediaObject : HypermediaObject
    {
        
    }
}
