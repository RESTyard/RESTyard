using System.Reflection;

namespace WebApi.HypermediaExtensions.WebApi.Siren.Model
{
    public class SirenProperty
    {

        public string AsPropertyName { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }

        public SirenProperty(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            AsPropertyName = propertyInfo.Name;
        }
        public SirenProperty(PropertyInfo propertyInfo, string asPropertyName)
        {
            PropertyInfo = propertyInfo;
            AsPropertyName = asPropertyName;
        }
        
    }
}