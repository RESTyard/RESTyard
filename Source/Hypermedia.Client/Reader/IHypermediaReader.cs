using System.IO;
using System.Threading.Tasks;
using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Resolver;

namespace Bluehands.Hypermedia.Client.Reader
{
    public interface IHypermediaReader
    {
        HypermediaClientObject Read(
            string contentString,
            IHypermediaResolver resolver);

        Task<HypermediaClientObject> ReadAsync(
            Stream contentStream,
            IHypermediaResolver resolver);

        Task<(HypermediaClientObject, string)> ReadAndSerializeAsync(
            Stream contentStream,
            IHypermediaResolver resolver);
    }
}