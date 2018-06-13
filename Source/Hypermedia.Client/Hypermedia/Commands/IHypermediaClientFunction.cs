namespace Hypermedia.Client.Hypermedia.Commands
{
    using System.Threading.Tasks;

    public interface IHypermediaClientFunction<TResultType> : IHypermediaClientCommand where TResultType : HypermediaClientObject
    {
        Task<HypermediaFunctionResult<TResultType>> ExecuteAsync();
    }

    public interface IHypermediaClientFunction<TResultType, TParameters> : IHypermediaClientCommand where TResultType : HypermediaClientObject
    {
        Task<HypermediaFunctionResult<TResultType>> ExecuteAsync(TParameters parameters);
    }
}