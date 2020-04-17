using System;
using System.Collections.Generic;
using System.Reflection;
using FunicularSwitch;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Links;
using WebApi.HypermediaExtensions.Util.Extensions;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.WebApi.Serializer.Model
{
    public class Link : LinkBase
    {
        public Link(List<Relation> relations, PropertyInfo propertyInfo, Title title) : base(relations, propertyInfo, title)
        {
            GetValue = PropertyInfo.GetValueGetter<HypermediaObjectReferenceBase>();
        }

        protected Func<object, HypermediaObjectReferenceBase> GetValue { get; }

        public override Result<string> ResolveUrl(IHypermediaRouteResolver resolver, object hypermediaObject)
        {
            var referenceBase = GetValue.Invoke(hypermediaObject);
            return referenceBase != null ? Result.Ok(resolver.ReferenceToRoute(referenceBase)) : Result.Error<string>(""); // todo value was null message
        }
    }
}