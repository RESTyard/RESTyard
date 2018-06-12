namespace Hypermedia.Client.Hypermedia.Commands
{
    using System;
    using System.Threading.Tasks;

    public class HypermediaClientAction : HypermediaClientCommandBase, IHypermediaClientAction
    {
        public HypermediaClientAction()
        {
            this.HasParameters = false;
            this.HasResultLink = false;
        }

        public Task<HypermediaCommandResult> ExecuteAsync()
        {
            if (!this.CanExecute)
            {
                throw new Exception("Can not execute Action.");
            }

            return this.Resolver.ResolveActionAsync(this.Uri, this.Method);
        }
    }

    public class HypermediaClientAction<TParameters> : HypermediaClientCommandBase, IHypermediaClientAction<TParameters>
    {
        public HypermediaClientAction()
        {
            this.HasParameters = true;
            this.HasResultLink = false;
        }

        public Task<HypermediaCommandResult> ExecuteAsync(TParameters parameters)
        {
            if (!this.CanExecute)
            {
                throw new Exception("Can not execute Action.");
            }

            return this.Resolver.ResolveActionAsync(this.Uri, this.Method, this.ParameterDescriptions, parameters);
        }
    }
}