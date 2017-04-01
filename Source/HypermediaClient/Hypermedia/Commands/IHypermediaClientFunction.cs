using System.Threading.Tasks;

namespace HypermediaClient.Hypermedia.Commands
{
    public interface IHypermediaClientFunction<TResultType> : IHypermediaClientCommand where TResultType : HypermediaClientObject
    {
        Task<HypermediaFunctionResult<TResultType>> ExecuteAsync();
    }

    public interface IHypermediaClientFunction<TResultType, TParameters> : IHypermediaClientCommand where TResultType : HypermediaClientObject
    {
        Task<HypermediaFunctionResult<TResultType>> ExecuteAsync(TParameters parameters);
    }
}