﻿using System;
using System.Collections.Generic;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.Query;
using RESTyard.Relations;

namespace RESTyard.AspNetCore.Hypermedia
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
        /// <summary>
        /// Constructor for HypermediaObject
        /// </summary>
        protected HypermediaObject(bool hasSelfLink = true)
        {
            if (hasSelfLink)
            {
                Links.Add(DefaultHypermediaRelations.Self, new HypermediaObjectReference(this));
            }
        }

        /// <summary>
        /// Constructor for HypermediaObjects which are accessed using a query. Required to properly build the self Link.
        /// </summary>
        protected HypermediaObject(IHypermediaQuery query)
        {
            // hypemedia object is a queryresult so it needs a selflink with query
            Links.Add(DefaultHypermediaRelations.Self, new HypermediaObjectQueryReference(GetType(), query));
        }

        /// <summary>
        /// List of embedded or linked entities which will be included in the formatted output.
        /// </summary>
        [FormatterIgnoreHypermediaProperty]
        public List<RelatedEntity> Entities { get; set; } = new();

        /// <summary>
        /// List of links to other HypermediaObjects which will be included in the formatted output.
        /// The key describes the relation to the linked HypermediaObject.
        /// </summary>
        [FormatterIgnoreHypermediaProperty]
        public RelationDictionary Links { get; } = new();
    }
}
