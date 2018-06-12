namespace Hypermedia.Client
{
    using global::Hypermedia.Client.Hypermedia;

    public interface IHypermediaReader
    {
        HypermediaClientObject Read(string contentString);
    }
}