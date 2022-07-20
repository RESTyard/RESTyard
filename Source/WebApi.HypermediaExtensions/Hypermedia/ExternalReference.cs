using System;
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
    public class ExternalReference : DirectReferenceBase<ExternalReference>
    {
        /// <summary>
        /// Creates a external link
        /// </summary>
        /// <param name="externalUri">The external URI</param>
        public ExternalReference(Uri externalUri) : base()
        {
            ExternalUri = externalUri;
        }

        [FormatterIgnoreHypermediaProperty]
        public Uri ExternalUri { get; set; }
    }
}