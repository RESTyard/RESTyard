namespace Hypermedia.Client.Hypermedia.Commands
{
    using System;
    using System.Threading.Tasks;

    public class HypermediaClientFunction<TResultType> : HypermediaClientCommandBase, IHypermediaClientFunction<TResultType> where TResultType : HypermediaClientObject
    {
        public HypermediaClientFunction()
        {
            this.HasParameters = false;
            this.HasResultLink = true;
        }

        public Task<HypermediaFunctionResult<TResultType>> ExecuteAsync()
        {
            if (!this.CanExecute)
            {
                throw new Exception("Can not execute Action.");
            }

            return this.Resolver.ResolveFunctionAsync<TResultType>(this.Uri, this.Method);
        }
    }

    public class HypermediaClientFunction<TResultType, TParameters> : HypermediaClientCommandBase, IHypermediaClientFunction<TResultType, TParameters> where TResultType : HypermediaClientObject
    {
        public HypermediaClientFunction()
        {
            this.HasParameters = true;
            this.HasResultLink = true;
        }

        public Task<HypermediaFunctionResult<TResultType>> ExecuteAsync(TParameters parameters)
        {
            if (!this.CanExecute)
            {
                throw new Exception("Can not execute Action.");
            }

            return this.Resolver.ResolveFunctionAsync<TResultType>(this.Uri, this.Method, this.ParameterDescriptions, parameters);
        }
    }
}