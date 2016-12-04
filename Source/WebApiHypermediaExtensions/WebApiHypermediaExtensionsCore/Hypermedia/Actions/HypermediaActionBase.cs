using System;

namespace WebApiHypermediaExtensionsCore.Hypermedia.Actions
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

        public abstract Type ParameterType();
    }
}