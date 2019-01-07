using System.Threading.Tasks;
using Bluehands.Hypermedia.Client.Hypermedia;

namespace Bluehands.Hypermedia.Client
{
    public interface IHypermediaClient<TEntryPoint> where TEntryPoint : HypermediaClientObject
    {
        Task<TEntryPoint> EnterAsync();
    }
}