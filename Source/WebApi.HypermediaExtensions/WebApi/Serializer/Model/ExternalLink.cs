using System;
using System.Collections.Generic;
using System.Reflection;
using FunicularSwitch;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Util.Extensions;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.WebApi.Serializer.Model
{
    public class ExternalLink : LinkBase
    {
        public ExternalLink(List<Relation> relations, PropertyInfo propertyInfo, Title title) : base(relations, propertyInfo, title)
        {
            GetValue = propertyInfo.GetValueGetter<Uri>(); // todo remove cast and replace function by compiled expression
        }

        protected Func<object, Uri> GetValue { get; }


        public override Result<string> ResolveUrl(IHypermediaRouteResolver resolver, object hypermediaObject)
        {
            var uri = GetValue.Invoke(hypermediaObject);
            return uri != null ? Result.Ok(uri.ToString()) : Result.Error<string>(""); // todo value was null message
        }
    }
}