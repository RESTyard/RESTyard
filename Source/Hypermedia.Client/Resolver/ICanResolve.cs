namespace Bluehands.Hypermedia.Client.Resolver
{
    public interface ICanResolve
    {
        IHypermediaResolver Resolver { get; set; }
    }
}