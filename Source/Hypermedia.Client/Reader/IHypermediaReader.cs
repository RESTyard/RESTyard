using System;
using System.IO;
using System.Threading.Tasks;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Resolver;

namespace RESTyard.Client.Reader
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