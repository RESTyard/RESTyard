namespace Hypermedia.Client.Resolver
{
    using global::Hypermedia.Client.Hypermedia;

    public class ResolverResult<T> where T : HypermediaClientObject
    {
        public bool Success { get; set; }
        public T ResultObject { get; set; } 
    }
}