using System.Threading.Tasks;

namespace Bluehands.Hypermedia.Client.Hypermedia.Commands
{
    public interface IHypermediaClientAction : IHypermediaClientCommand
    {
        Task<HypermediaCommandResult> ExecuteAsync();
        
    }

    public interface IHypermediaClientAction<TParameters> : IHypermediaClientCommand
    {
        Task<HypermediaCommandResult> ExecuteAsync(TParameters parameters);
    }
}