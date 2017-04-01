using System;
using System.Threading.Tasks;

namespace HypermediaClient.Hypermedia.Commands
{
    public class HypermediaClientAction : HypermediaClientCommandBase, IHypermediaClientAction
    {
        public HypermediaClientAction()
        {
            HasParameters = false;
            HasResultLink = false;
        }

        public Task<HypermediaCommandResult> ExecuteAsync()
        {
            if (!CanExecute)
            {
                throw new Exception("Can not execute Action.");
            }

            return Resolver.ResolveActionAsync(Uri, Method);
        }
    }

    public class HypermediaClientAction<TParameters> : HypermediaClientCommandBase, IHypermediaClientAction<TParameters>
    {
        public HypermediaClientAction()
        {
            HasParameters = true;
            HasResultLink = false;
        }

        public Task<HypermediaCommandResult> ExecuteAsync(TParameters parameters)
        {
            if (!CanExecute)
            {
                throw new Exception("Can not execute Action.");
            }

            return Resolver.ResolveActionAsync(Uri, Method, ParameterDescriptions, parameters);
        }
    }
}