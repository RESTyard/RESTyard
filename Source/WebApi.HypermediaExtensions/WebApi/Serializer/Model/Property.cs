using System.Reflection;

namespace WebApi.HypermediaExtensions.WebApi.Serializer.Model
{
    public class Property
    {

        public Property(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            SerializationName = propertyInfo.Name;
        }
        public Property(PropertyInfo propertyInfo, string serializationName)
        {
            PropertyInfo = propertyInfo;
            SerializationName = serializationName;
        }

        public string SerializationName { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }

    }
}