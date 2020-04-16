using System.Collections.Generic;
using Bluehands.Hypermedia.Relations;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.Hypermedia.Links;
using WebApi.HypermediaExtensions.Query;

namespace WebApi.HypermediaExtensions.Hypermedia
{
    /// <summary>
    /// Base class for all HypermediaObjects. Can be attributed with a <see cref="HypermediaObjectAttribute" /> to provide classes and a title.
    /// 
    /// Public properties: will be included in the formatted Hypermedia. The value will be determined by calling ToString().
    /// A property can be anottated by <see cref="Property" /> to give it another name in the output.
    /// If a Property shall not be included add <see cref="FormatterIgnore" />.
    /// 
    /// Actions: Properties which are HypermediaActions will be formatted as "Actions".
    /// Derive from <see cref="HypermediaAction"/> or use a generic <see cref="HypermediaAction{T}" />.
    /// The resulting Type must be unique and a matching attributed route must be provided.
    /// 
    /// Formatters may choose not to serialize actions which return CanExecute() == false.
    /// Can be annotated by <see cref="Action" /> to give it a fixed name and title for formatters
    /// If you do not wish a serialization use <see cref="FormatterIgnore" />
    /// </summary>
    public abstract class HypermediaObject
    {
        /// <summary>
        /// Constructor for HypermediaObject
        /// </summary>
        protected HypermediaObject(bool hasSelfLink = true)
        {
            Init();

            if (hasSelfLink)
            {
                SelfReference = new HypermediaObjectReference<HypermediaObject>(this);
            }
        }

        /// <summary>
        /// Constructor for HypermediaObject which are acces using a query. Required to propperly build the self Link.
        /// </summary>
        protected HypermediaObject(IHypermediaQuery query)
        {
            Init();
            // hypemedia object is a queryresult so it needs a selflink with query

           // SelfQueryReference = new HypermediaObjectQueryReference<HypermediaObject>(query);
        }

        private void Init()
        {
            Entities = new List<RelatedEntity>();
            // Links = new RelationDictionary();
        }

        /// <summary>
        /// List of embedded or linked entities which will be included in the formatted output.
        /// </summary>
        [FormatterIgnore]
        public List<RelatedEntity> Entities { get; set; }

        [Link(DefaultHypermediaRelations.Self)]
        public HypermediaObjectReference<HypermediaObject> SelfReference { get; set; }

//        [Link(DefaultHypermediaRelations.Self)]
        //public HypermediaObjectQueryReference<HypermediaObject> SelfQueryReference { get; set; }

        /// <summary>
        /// List of links to other HypermediaObjects which will be included in the formatted output.
        /// The key describes the relation to the linked HypermediaObject.
        /// </summary>
        //[FormatterIgnoreHypermediaProperty]
        //public RelationDictionary Links { get; private set; }
    }
}
