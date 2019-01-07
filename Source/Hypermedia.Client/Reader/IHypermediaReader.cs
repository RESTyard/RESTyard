using Bluehands.Hypermedia.Client.Hypermedia;

namespace Bluehands.Hypermedia.Client.Reader
{
    public interface IHypermediaReader
    {
        HypermediaClientObject Read(string contentString);
    }
}