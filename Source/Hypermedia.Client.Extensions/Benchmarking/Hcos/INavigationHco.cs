using Bluehands.Hypermedia.Client.Hypermedia;

namespace Benchmarking.Hcos
{
    public interface INavigationHco<THco>
        where THco : HypermediaClientObject
    {
        IHypermediaNavigationCommand<THco> Previous { get; }

        IHypermediaNavigationCommand<THco> Next { get; }
    }
}