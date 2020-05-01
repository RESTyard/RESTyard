using Bluehands.Hypermedia.Model;
using FunicularSwitch;
using WebApi.HypermediaExtensions.WebApi.Serializer.Reflection;

namespace WebApi.Hypermedia.ModelFactory.Test.ObjectReflection
{
    public abstract class ModelFactoryTestBase : TestSpecification
    {
        public override void Given()
        {
            this.ObjectReflectionService = new ObjectReflectionService();
        }

        public ObjectReflectionService ObjectReflectionService { get;  private set; }

        public Result<Entity> Result { get; set; }

    }
}
