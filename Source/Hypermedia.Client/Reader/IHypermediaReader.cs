using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Resolver;

namespace Bluehands.Hypermedia.Client.Reader
{
    public interface IHypermediaReader
    {
        void InitializeHypermediaResolver(IHypermediaResolver resolver);

        HypermediaClientObject Read(string contentString);
    }
}