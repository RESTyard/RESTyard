using System.Collections.Generic;
using System.Reflection;

namespace WebApi.HypermediaExtensions.WebApi.Siren.Model
{
    public class SirenAction
    {

        public SirenAction(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            AsPropertyName = propertyInfo.Name;
        }

        public SirenAction(PropertyInfo propertyInfo, SirenTitle title = null, string asPropertyName = "")
        {
            PropertyInfo = propertyInfo;
            Title = title;
            AsPropertyName = !string.IsNullOrEmpty(asPropertyName) ? asPropertyName : propertyInfo.Name;
        }

        public string AsPropertyName { get; private set; }

        public SirenTitle Title { get; private set; }   

        public PropertyInfo PropertyInfo { get; private set; }
    }
}