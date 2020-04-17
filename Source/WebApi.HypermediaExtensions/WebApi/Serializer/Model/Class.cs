namespace WebApi.HypermediaExtensions.WebApi.Serializer.Model
{
    public class Class
    {
        public Class(string typeName)
        {
            TypeName = typeName;
        }

        public string TypeName { get; private set; }
    }
}