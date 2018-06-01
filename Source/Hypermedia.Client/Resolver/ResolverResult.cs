using HypermediaClient.Hypermedia;

namespace HypermediaClient.Resolver
{
    public class ResolverResult<T> where T : HypermediaClientObject
    {
        public bool Success { get; set; }
        public T ResultObject { get; set; } 
    }
}