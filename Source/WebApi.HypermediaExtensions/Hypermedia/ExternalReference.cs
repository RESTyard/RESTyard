using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Mvc.Formatters;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.Hypermedia.Links;

namespace WebApi.HypermediaExtensions.Hypermedia
{
    /// <summary>
    /// This class is intended for situations where a reference to a route or external URI
    /// must be provided which can not be resolved by the framework. It can only be used
    /// in combination with <see cref="HypermediaObjectReference"/>
    /// Serializer will assume a GET HTTP method
    /// </summary>
    [HypermediaObject(Classes = new[] { "External" })]
    public class ExternalReference : HypermediaObject
    {
        /// <summary>
        /// Creates a external link
        /// </summary>
        /// <param name="externalUri">The external URI</param>
        public ExternalReference(Uri externalUri) : base(false)
        {
            ExternalUri = externalUri;
            AvailableMediaTypes = new Collection<string>();
        }
        
        /// <summary>
        /// Creates a external link
        /// </summary>
        /// <param name="externalUri">The external URI</param>
        /// <param name="availableMediaTypes">A list of media types that the resource provides. This is intended
        /// for situations where a client wants to switch media type e.g. a file download</param>
        public ExternalReference(Uri externalUri, IReadOnlyCollection<string> availableMediaTypes = null) : base(false)
        {
            ExternalUri = externalUri;
            AvailableMediaTypes = availableMediaTypes ?? new Collection<string>();
        }
        
        /// <summary>
        /// Creates a external link
        /// </summary>
        /// <param name="externalUri">The external URI</param>
        /// <param name="availableMediaType">A media type that the resource provides. This is intended
        /// for situations where a client wants to switch media type e.g. a file download</param>
        public ExternalReference(Uri externalUri, string availableMediaType = null) : base(false)
        {
            ExternalUri = externalUri;
            AvailableMediaTypes = availableMediaType != null ? new Collection<string>{availableMediaType} : new Collection<string>();
        }

        [FormatterIgnoreHypermediaProperty]
        public Uri ExternalUri { get; set; }

        [FormatterIgnoreHypermediaProperty]
        public IReadOnlyCollection<string> AvailableMediaTypes { get; }
    }
}