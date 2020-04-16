using System.Reflection;

namespace WebApi.HypermediaExtensions.WebApi.Serializer.Model
{
    public class ModelProperty
    {

        public ModelProperty(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            SerializationName = propertyInfo.Name;
        }
        public ModelProperty(PropertyInfo propertyInfo, string serializationName)
        {
            PropertyInfo = propertyInfo;
            SerializationName = serializationName;
        }

        public string SerializationName { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }

    }
}