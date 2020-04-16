using System.Reflection;

namespace WebApi.HypermediaExtensions.WebApi.Siren.Model
{
    public class SirenProperty
    {

        public SirenProperty(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            SerializationName = propertyInfo.Name;
        }
        public SirenProperty(PropertyInfo propertyInfo, string serializationName)
        {
            PropertyInfo = propertyInfo;
            SerializationName = serializationName;
        }

        public string SerializationName { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }

    }
}