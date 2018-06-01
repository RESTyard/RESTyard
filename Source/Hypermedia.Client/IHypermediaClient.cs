using System.Threading.Tasks;
using HypermediaClient.Hypermedia;

namespace HypermediaClient
{
    public interface IHypermediaClient<TEntryPoint> where TEntryPoint : HypermediaClientObject
    {
        Task<TEntryPoint> EnterAsync();
    }
}