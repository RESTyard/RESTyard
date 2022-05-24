using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Hypermedia.Attributes;
using Bluehands.Hypermedia.Client.Hypermedia.Commands;

namespace Benchmarking.Hcos
{
    public interface IHypermediaNavigationCommand<THco>
        where THco : HypermediaClientObject
    {
        IHypermediaClientFunction<THco> Navigate { get; set; }
    }

    public abstract class HypermediaNavigationCommand<THco>
        : HypermediaClientObject, IHypermediaNavigationCommand<THco>
        where THco : HypermediaClientObject
    {
        [HypermediaCommand("Navigate")]
        public IHypermediaClientFunction<THco> Navigate { get; set; }
    }
}