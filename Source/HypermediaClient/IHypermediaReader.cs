namespace HypermediaClient
{
    using HypermediaClient.Hypermedia;

    public interface IHypermediaReader
    {
        HypermediaClientObject Read(string contentString);
    }
}