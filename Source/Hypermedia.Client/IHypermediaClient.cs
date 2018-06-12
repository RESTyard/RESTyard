namespace Hypermedia.Client
{
    using System.Threading.Tasks;

    using global::Hypermedia.Client.Hypermedia;

    public interface IHypermediaClient<TEntryPoint> where TEntryPoint : HypermediaClientObject
    {
        Task<TEntryPoint> EnterAsync();
    }
}