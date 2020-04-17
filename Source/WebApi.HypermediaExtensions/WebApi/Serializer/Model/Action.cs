using System.Reflection;

namespace WebApi.HypermediaExtensions.WebApi.Serializer.Model
{
    public class Action
    {

        public Action(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            SerializationName = propertyInfo.Name;
        }

        public Action(PropertyInfo propertyInfo, Title title = null, string asPropertyName = "")
        {
            PropertyInfo = propertyInfo;
            Title = title;
            SerializationName = !string.IsNullOrEmpty(asPropertyName) ? asPropertyName : propertyInfo.Name;
        }

        public string SerializationName { get; private set; }

        public Title Title { get; private set; }   

        public PropertyInfo PropertyInfo { get; private set; }
    }
}