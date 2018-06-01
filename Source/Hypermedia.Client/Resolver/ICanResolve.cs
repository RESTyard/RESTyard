namespace HypermediaClient.Resolver
{
    public interface ICanResolve
    {
        IHypermediaResolver Resolver { get; set; }
    }
}