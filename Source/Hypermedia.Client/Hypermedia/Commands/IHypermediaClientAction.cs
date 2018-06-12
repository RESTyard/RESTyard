namespace Hypermedia.Client.Hypermedia.Commands
{
    using System.Threading.Tasks;

    public interface IHypermediaClientAction : IHypermediaClientCommand
    {
        Task<HypermediaCommandResult> ExecuteAsync();
        
    }

    public interface IHypermediaClientAction<TParameters> : IHypermediaClientCommand
    {
        Task<HypermediaCommandResult> ExecuteAsync(TParameters parameters);
    }
}