using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.Hypermedia.Links
{
    /// <summary>
    /// This class is intended for referencing a external hypermedia object e.g. on another server.
    /// It is intended to be used as EmbeddedEntity.
    /// </summary>
    public class HypermediaExternalObjectReference : HypermediaObjectReferenceBase
    {
        public HypermediaExternalObjectReference(Uri uri, IEnumerable<string> classes) : base(typeof(ExternalObject))
        {
            Uri = uri;
            Classes = classes?.ToImmutableList() ?? ImmutableList<string>.Empty;
        }

        public Uri Uri { get; }

        public ImmutableList<string> Classes { get; }

        public override bool CanResolve()
        {
            return false;
        }

        public override IHypermediaObject? GetInstance()
        {
            return null;
        }

        public override bool IsResolved()
        {
            return false;
        }

        public override object? GetKey(IKeyProducer keyProducer)
        {
            throw new NotSupportedException();
        }

        public override IHypermediaQuery? GetQuery()
        {
            return null;
        }

        class ExternalObject : IHypermediaObject
        {

        }
    }
}