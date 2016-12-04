﻿using System.Collections.Generic;
using WebApiHypermediaExtensionsCore.Hypermedia.Actions;
using WebApiHypermediaExtensionsCore.Hypermedia.Attributes;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;
using WebApiHypermediaExtensionsCore.Query;

namespace WebApiHypermediaExtensionsCore.Hypermedia
{
    /// <summary>
    /// Base class for all HypermediaObjects. Can be attributed with a <see cref="HypermediaObjectAttribute" /> to provide classes and a title.
    /// 
    /// Public properties: will be included in the formatted Hypermedia. The value will be determined by calling ToString().
    /// A property can be anottated by <see cref="HypermediaPropertyAttribute" /> to give it another name in the output.
    /// If a Property shall not be included add <see cref="FormatterIgnoreHypermediaPropertyAttribute" />.
    /// 
    /// Actions: Properties which are HypermediaActions will be formatted as "Actions".
    /// Derive from <see cref="HypermediaAction"/> or use a generic <see cref="HypermediaAction{T}" />.
    /// The resulting Type must be unique and a matching attributed route must be provided.
    /// 
    /// Formatters may choose not to serialize actions which return CanExecute() == false.
    /// Can be annotated by <see cref="HypermediaActionAttribute" /> to give it a fixed name and title for formatters
    /// If you do not wish a serialization use <see cref="FormatterIgnoreHypermediaPropertyAttribute" />
    /// </summary>
    public abstract class HypermediaObject
    {
        protected HypermediaObject()
        {
            Entities = new List<HypermediaObjectReferenceBase>();
            Links = new Dictionary<string, HypermediaObjectReferenceBase>();

            Links[DefaultHypermediaLinks.Self] = new HypermediaObjectReference(this);

        }

        protected HypermediaObject(IHypermediaQuery query)
        {
            Entities = new List<HypermediaObjectReferenceBase>();
            Links = new Dictionary<string, HypermediaObjectReferenceBase>();

            // hypemedia object is a queryresult so it needs a selflink with query
            Links[DefaultHypermediaLinks.Self] = new HypermediaObjectQueryReference(GetType(), query);

        }

        /// <summary>
        /// List of embedded or linked entities which will be included in the formatted output.
        /// </summary>
        [FormatterIgnoreHypermediaProperty]
        public List<HypermediaObjectReferenceBase> Entities { get; set; }

        /// <summary>
        /// List of links to other HypermediaObjects which will be included in the formatted output.
        /// </summary>
        [FormatterIgnoreHypermediaProperty]
        public Dictionary<string, HypermediaObjectReferenceBase> Links { get; }
    }
}
