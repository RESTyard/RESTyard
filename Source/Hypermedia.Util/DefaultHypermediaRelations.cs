namespace Hypermedia.Util
{
    /// <summary>
    /// Collection of basic relations commonly used.
    /// For a comprehensive list <see href="https://www.iana.org/assignments/link-relations/link-relations.xhtml"/>
    /// </summary>
    public static class DefaultHypermediaRelations
    {
        /// <summary>
        /// Relation indicating that this relates to the HypermediaObject itselve.
        /// </summary>
        public const string Self = "Self";

        /// <summary>
        /// Relations commonly used for query results.
        /// </summary>
        public class Queries
        {
            public const string First = "First";
            public const string Previous = "Previous";
            public const string Next = "Next";
            public const string Last = "Last";
            public const string All = "All";
        }


        /// <summary>
        /// Relations commonly used for embedded entities.
        /// </summary>
        public class EmbeddedEntities
        {
            /// <summary>
            /// Indicates that the embedded Entity is a collection or list item.
            /// </summary>
            public const string Item = "Item";
            public const string Parent = "Parent";
            public const string Child = "Child";
        }
    }
    
}