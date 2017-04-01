using System;
using System.Threading.Tasks;

namespace HypermediaClient.Hypermedia.Commands
{
    public class HypermediaClientFunction<TResultType> : HypermediaClientCommandBase, IHypermediaClientFunction<TResultType> where TResultType : HypermediaClientObject
    {
        public HypermediaClientFunction()
        {
            HasParameters = false;
            HasResultLink = true;
        }

        public Task<HypermediaFunctionResult<TResultType>> ExecuteAsync()
        {
            if (!CanExecute)
            {
                throw new Exception("Can not execute Action.");
            }

            return Resolver.ResolveFunctionAsync<TResultType>(Uri, Method);
        }
    }

    public class HypermediaClientFunction<TResultType, TParameters> : HypermediaClientCommandBase, IHypermediaClientFunction<TResultType, TParameters> where TResultType : HypermediaClientObject
    {
        public HypermediaClientFunction()
        {
            HasParameters = true;
            HasResultLink = true;
        }

        public Task<HypermediaFunctionResult<TResultType>> ExecuteAsync(TParameters parameters)
        {
            if (!CanExecute)
            {
                throw new Exception("Can not execute Action.");
            }

            return Resolver.ResolveFunctionAsync<TResultType>(Uri, Method, ParameterDescriptions, parameters);
        }
    }
}