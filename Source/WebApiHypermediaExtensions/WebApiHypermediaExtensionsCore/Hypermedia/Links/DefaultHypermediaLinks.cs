namespace WebApiHypermediaExtensionsCore.Hypermedia.Links
{

    // collection of basic links
    // for links only used by a derived HypermediObject also derive from this
    public static class DefaultHypermediaLinks
    {
        public const string Self = "Self";

        // querries
        public class Queries
        {
            public const string First = "First";
            public const string Previous = "Previous";
            public const string Next = "Next";
            public const string Last = "Last";
            public const string All = "All";
        }
    }
    
}