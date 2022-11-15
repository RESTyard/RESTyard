namespace RESTyard.Util.Relations
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
        public const string Self = "self";

        /// <summary>
        /// Relations commonly used for query results.
        /// </summary>
        public class Queries
        {
            public const string First = "first";
            public const string Previous = "previous";
            public const string Next = "next";
            public const string Last = "last";
            public const string All = "all";
        }


        /// <summary>
        /// Relations commonly used for embedded entities.
        /// </summary>
        public class EmbeddedEntities
        {
            /// <summary>
            /// Indicates that the embedded Entity is a collection or list item.
            /// </summary>
            public const string Item = "item";
            public const string Parent = "parent";
            public const string Child = "child";
        }
    }
    
}