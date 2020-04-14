namespace WebApi.HypermediaExtensions.WebApi.Siren.Model
{
    public class SirenClass
    {
        public SirenClass(string typeName)
        {
            TypeName = typeName;
        }

        public string TypeName { get; private set; }
    }
}