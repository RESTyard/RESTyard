using System;
using System.Diagnostics.CodeAnalysis;

namespace RESTyard.AspNetCore.Hypermedia.Actions
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

        public virtual bool TryGetParameterType([NotNullWhen(true)] out Type? parameterType)
        {
            parameterType = ParameterType;
            return parameterType is not null;
        }
        
        public abstract object? GetPrefilledParameter();

        protected abstract Type? ParameterType { get; }
    }
}