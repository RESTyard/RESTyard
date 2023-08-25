using System;
using System.IO;
using System.Threading.Tasks;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Resolver;

namespace RESTyard.Client.Reader
{
    public interface IHypermediaReader
    {
        HypermediaReaderResult<HypermediaClientObject> Read(
            string contentString,
            IHypermediaResolver resolver);

        Task<HypermediaReaderResult<HypermediaClientObject>> ReadAsync(
            Stream contentStream,
            IHypermediaResolver resolver);

        Task<HypermediaReaderResult<(HypermediaClientObject, string)>> ReadAndSerializeAsync(
            Stream contentStream,
            IHypermediaResolver resolver);
    }
}