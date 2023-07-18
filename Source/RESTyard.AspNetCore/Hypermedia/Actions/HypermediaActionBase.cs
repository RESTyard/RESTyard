﻿using System;

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

        public bool HasParameter() => ParameterType() != null;
        
        public abstract object? GetPrefilledParameter();

        public abstract Type? ParameterType();
    }
}