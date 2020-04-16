namespace WebApi.HypermediaExtensions.WebApi.Serializer.Model
{
    public class ModelClass
    {
        public ModelClass(string typeName)
        {
            TypeName = typeName;
        }

        public string TypeName { get; private set; }
    }
}