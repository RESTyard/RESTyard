using System.Collections.Generic;
using System.Reflection;
using FunicularSwitch;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.WebApi.Serializer.Model
{
    public abstract class LinkBase : ILink
    {
        protected LinkBase(List<Relation> relations, PropertyInfo propertyInfo, Title title)
        {
            Relations = relations;
            PropertyInfo = propertyInfo;
            Title = title;
        }

        public Title Title { get; private set; }
        public List<Relation> Relations { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }
        public abstract Result<string> ResolveUrl(IHypermediaRouteResolver resolver, object hypermediaObject);
    }
}