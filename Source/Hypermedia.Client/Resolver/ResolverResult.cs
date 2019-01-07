using Bluehands.Hypermedia.Client.Hypermedia;

namespace Bluehands.Hypermedia.Client.Resolver
{
    public class ResolverResult<T> where T : HypermediaClientObject
    {
        public bool Success { get; set; }
        public T ResultObject { get; set; } 
    }
}