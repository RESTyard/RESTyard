using System;
using System.Reflection;
using WebApi.HypermediaExtensions.Exceptions;
using WebApi.HypermediaExtensions.Query;

namespace WebApi.HypermediaExtensions.Hypermedia
{
    public class HypermediaQueryLocation
    {
        public Type QueryType { get; set; }
        public IHypermediaQuery QueryParameter { get; set; }

        public HypermediaQueryLocation(Type queryType, IHypermediaQuery queryParameter = null)
        {
            if (!typeof(HypermediaQueryResult).GetTypeInfo().IsAssignableFrom(queryType))
            {
                throw new HypermediaQueryException($"HypermediaQueryLocation requires a type derived from '{typeof(HypermediaQueryResult)}'");
            }

            QueryType = queryType;
            QueryParameter = queryParameter;
        }
    }
}