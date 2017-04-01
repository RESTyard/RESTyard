using System;
using WebApiHypermediaExtensionsCore.Exceptions;

namespace WebApiHypermediaExtensionsCore.Hypermedia.Actions
{
    /// <summary>
    /// A HypermediaAction. For each concrete type a corresponding attributed route must exist.
    /// </summary>
    /// <typeparam name="TParameter">Parameter object passed to the Action.</typeparam>
    public class HypermediaAction<TParameter> : HypermediaActionBase where TParameter : IHypermediaActionParameter
    {
        private readonly Action<TParameter> command;

        public HypermediaAction(Func<bool> canExecute, Action<TParameter> command = null) : base(canExecute)
        {
            this.command = command;
        }

        public void Execute(TParameter parameter)
        {
            if (!CanExecute())
            {
                throw new CanNotExecuteActionException("Can not execute.");
            }

            if (command == null)
            {
                throw new NoActionSetException($"No Action set: '{GetType()}'");
            }

            command(parameter);
        }

        public override bool HasParameter()
        {
            return true;
        }

        public override Type ParameterType()
        {
            return typeof(TParameter);
        }
    }

    /// <summary>
    /// A HypermediaAction. For each concrete type a corresponding attributed route must exist.
    /// </summary>
    public abstract class HypermediaAction : HypermediaActionBase
    {
        private readonly Action command;

        protected HypermediaAction(Func<bool> canExecute, Action command) : base(canExecute)
        {
            this.command = command;
        }

        public void Execute()
        {
            if (!CanExecute())
            {
                throw new CanNotExecuteActionException("Can not execute.");
            }

            if (command == null)
            {
                throw new NoActionSetException($"No Action set: '{GetType()}'");
            }

            command();
        }

        public override bool HasParameter()
        {
            return false;
        }

        public override Type ParameterType()
        {
            return null;
        }
    }
}
