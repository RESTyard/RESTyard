using System;

namespace RESTyard.WebApi.Extensions.Hypermedia.Actions
{
    public abstract class HypermediaActionBase
    {
        private readonly Func<bool> commandCanExecute;

        protected HypermediaActionBase(Func<bool> canExecute)
        {
            commandCanExecute = canExecute;
        }

        public bool CanExecute()
        {
            return commandCanExecute();
        }

        public abstract bool HasParameter();
        
        public abstract object GetPrefilledParameter();

        public abstract Type ParameterType();
    }
}