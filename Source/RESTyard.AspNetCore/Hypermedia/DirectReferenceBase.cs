using System;
using System.Collections.Generic;
using RESTyard.AspNetCore.Hypermedia.Attributes;

namespace RESTyard.AspNetCore.Hypermedia
{
    public class DirectReferenceBase<TDerived> : IHypermediaObject where TDerived : DirectReferenceBase<TDerived>
    {
        protected DirectReferenceBase() { }

        [FormatterIgnoreHypermediaProperty]
        public IReadOnlyCollection<string> AvailableMediaTypes { get; private set; } = Array.Empty<string>();
        
        /// <summary>
        /// Sets the media type that the resource provides. This is intended
        /// for situations where a client wants to switch media type e.g. a file download
        /// </summary>
        public TDerived WithAvailableMediaType(string availableMediaType)
        {
            this.AvailableMediaTypes = new[] { availableMediaType };
            return (TDerived)this;
        }
        
        /// <summary>
        /// Sets the list of media types that the resource provides. This is intended
        /// for situations where a client wants to switch media type e.g. a file download
        /// </summary>
        public TDerived WithAvailableMediaTypes(IReadOnlyCollection<string> availableMediaTypes)
        {
            this.AvailableMediaTypes = availableMediaTypes;
            return (TDerived)this;
        }
    }
}