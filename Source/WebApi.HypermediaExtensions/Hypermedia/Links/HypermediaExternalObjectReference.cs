using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using WebApi.HypermediaExtensions.Query;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.Hypermedia.Links
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