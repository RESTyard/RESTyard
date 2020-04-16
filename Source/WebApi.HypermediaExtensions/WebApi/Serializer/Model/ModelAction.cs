using System.Reflection;

namespace WebApi.HypermediaExtensions.WebApi.Serializer.Model
{
    public class ModelAction
    {

        public ModelAction(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            AsPropertyName = propertyInfo.Name;
        }

        public ModelAction(PropertyInfo propertyInfo, ModelTitle title = null, string asPropertyName = "")
        {
            PropertyInfo = propertyInfo;
            Title = title;
            AsPropertyName = !string.IsNullOrEmpty(asPropertyName) ? asPropertyName : propertyInfo.Name;
        }

        public string AsPropertyName { get; private set; }

        public ModelTitle Title { get; private set; }   

        public PropertyInfo PropertyInfo { get; private set; }
    }
}