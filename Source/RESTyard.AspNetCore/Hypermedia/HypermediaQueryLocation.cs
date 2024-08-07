﻿using System;
using System.Reflection;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Query;

namespace RESTyard.AspNetCore.Hypermedia
{
    public class HypermediaQueryLocation
    {
        public Type QueryType { get; set; }
        public IHypermediaQuery? QueryParameter { get; set; }

        public HypermediaQueryLocation(Type queryType, IHypermediaQuery? queryParameter = null)
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