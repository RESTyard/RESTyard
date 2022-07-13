using System.Threading.Tasks;

namespace Bluehands.Hypermedia.Client.Hypermedia.Commands
{
    public interface IHypermediaClientAction
        : IHypermediaClientCommand
    {
    }

    public interface IHypermediaClientAction<TParameters>
        : IHypermediaClientCommand
    {
    }
}