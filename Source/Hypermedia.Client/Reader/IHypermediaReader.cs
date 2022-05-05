using System.IO;
using System.Threading.Tasks;
using Bluehands.Hypermedia.Client.Hypermedia;

namespace Bluehands.Hypermedia.Client.Reader
{
    public interface IHypermediaReader
    {
        HypermediaClientObject Read(string contentString);

        Task<HypermediaClientObject> ReadAsync(Stream contentStream);
    }
}