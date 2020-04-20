using FunicularSwitch;
using WebApi.HypermediaExtensions.WebApi.Serializer.Reflection;

namespace WebApi.Hypermedia.Serializer.Tests.ObjectReflection
{
    public abstract class ObjectReflectionServiceTestBase : TestSpecification
    {
        public override void Given()
        {
            this.ObjectReflectionService = new ObjectReflectionService();
        }

        public ObjectReflectionService ObjectReflectionService { get;  private set; }

        public Result<HypermediaExtensions.WebApi.Serializer.Reflection.ObjectReflection> Result { get; set; }

    }
}
