using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.Hypermedia.Links
{
    public class HypermediaExternalObjectReference : HypermediaObjectReferenceBase
    {
        public HypermediaExternalObjectReference(Uri uri, IEnumerable<string> classes) : base(typeof(ExternalObject))
        {
            Uri = uri;
            Classes = classes?.ToImmutableList();
        }

        public Uri Uri { get; }

        public ImmutableList<string> Classes { get; }

        public override bool CanResolve()
        {
            return false;
        }

        public override HypermediaObject GetInstance()
        {
            return null;
        }

        public override bool IsResolved()
        {
            return false;
        }

        public override object GetKey(IKeyProducer keyProducer)
        {
            throw new NotSupportedException();
        }

        public override IHypermediaQuery GetQuery()
        {
            return null;
        }

        class ExternalObject : HypermediaObject
        {

        }
    }
}