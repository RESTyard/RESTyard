using System.Collections.Generic;
using System.Reflection;

namespace WebApi.HypermediaExtensions.WebApi.Serializer.Model
{
    public class ModelLink
    {
        public ModelLink(List<ModelRelation> relations, PropertyInfo propertyInfo, bool isExternal, ModelTitle title)
        {
            Relations = relations;
            PropertyInfo = propertyInfo;
            IsExternal = isExternal;
            Title = title;
        }

        public ModelTitle Title { get; private set; }

        public List<ModelRelation> Relations { get; private set; }

        public PropertyInfo PropertyInfo { get; private set; }

        public bool IsExternal { get; private set; }
    }
}