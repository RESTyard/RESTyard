using System.Collections.Generic;
using System.Reflection;

namespace WebApi.HypermediaExtensions.WebApi.Siren.Model
{
    public class SirenLink
    {
        public SirenLink(List<SirenRelation> relations, PropertyInfo propertyInfo, bool isExternal, SirenTitle title)
        {
            Relations = relations;
            PropertyInfo = propertyInfo;
            IsExternal = isExternal;
            Title = title;
        }

        public SirenTitle Title { get; private set; }

        public List<SirenRelation> Relations { get; private set; }

        public PropertyInfo PropertyInfo { get; private set; }

        public bool IsExternal { get; private set; }
    }
}