using System;
using WebApiHypermediaExtensionsCore.Hypermedia.Attributes;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;

namespace WebApiHypermediaExtensionsCore.Hypermedia
{
    /// <summary>
    /// This class is intended for situations where a reference to a route or external URI
    /// must be provided which can not be resolved by the framework. It can only be used
    /// in combination with <see cref="HypermediaObjectReference"/>
    /// </summary>
    [HypermediaObject(Classes = new []{"External"})]
    public class ExternalReference : HypermediaObject
    {
        public ExternalReference(Uri externalUri) : base(false)
        {
            ExternalUri = externalUri;
        }

        [FormatterIgnoreHypermediaProperty]
        public Uri ExternalUri { get; set; }
    }
}